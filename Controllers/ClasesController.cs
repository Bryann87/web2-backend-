using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using academia.Data;
using academia.Models;
using academia.DTOs;
using academia.Services;

namespace academia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClasesController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;

        public ClasesController(AcademiaContext context, IMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
        }

        private bool EsAdministrador() => User.FindFirst("rol")?.Value == "administrador";
        private int? ObtenerIdPersona()
        {
            // Intentar obtener el ID desde diferentes claims posibles
            var subClaim = User.FindFirst("sub")?.Value;
            var nameidClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var idStr = subClaim ?? nameidClaim;
            
            // Log para depuración
            Console.WriteLine($"[DEBUG] Claims disponibles: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            Console.WriteLine($"[DEBUG] sub={subClaim}, nameid={nameidClaim}, usando={idStr}");
            
            return int.TryParse(idStr, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ClaseDto>>>> GetClases(
            [FromQuery] PaginationParams pagination,
            [FromQuery] string? diaSemana = null,
            [FromQuery] int? idEstilo = null,
            [FromQuery] int? idProfesor = null,
            [FromQuery] bool? activa = null,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                IQueryable<Clase> query = _context.Clases
                    .Include(c => c.EstiloDanza)
                    .Include(c => c.Profesor)
                    .Include(c => c.Inscripciones);

                // Profesores solo ven sus clases
                if (!EsAdministrador())
                {
                    var idPersona = ObtenerIdPersona();
                    if (idPersona == null)
                        return StatusCode(403, ApiResponse<PaginatedResponse<ClaseDto>>.ErrorResponse("No autorizado"));
                    query = query.Where(c => c.IdProfesor == idPersona);
                }

                // Filtros
                if (!string.IsNullOrEmpty(diaSemana))
                    query = query.Where(c => c.DiaSemana == diaSemana);
                if (idEstilo.HasValue)
                    query = query.Where(c => c.IdEstilo == idEstilo.Value);
                if (idProfesor.HasValue)
                    query = query.Where(c => c.IdProfesor == idProfesor.Value);
                if (activa.HasValue)
                    query = query.Where(c => c.Activa == activa.Value);
                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(c => 
                        (c.NombreClase != null && c.NombreClase.Contains(busqueda)) ||
                        (c.Profesor != null && (c.Profesor.Nombre.Contains(busqueda) || c.Profesor.Apellido.Contains(busqueda))) ||
                        (c.EstiloDanza != null && c.EstiloDanza.NombreEsti.Contains(busqueda)));

                var totalRecords = await query.CountAsync();
                var clases = await query
                    .OrderBy(c => c.DiaSemana)
                    .ThenBy(c => c.Hora)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var clasesDto = clases.Select(c => _mappingService.ToDto(c));
                var paginatedResponse = new PaginatedResponse<ClaseDto>
                {
                    Data = clasesDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<ClaseDto>>.SuccessResponse(paginatedResponse, "Clases obtenidas exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<ClaseDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClaseDto>>> GetClase(int id)
        {
            try
            {
                var clase = await _context.Clases
                    .Include(c => c.EstiloDanza)
                    .Include(c => c.Profesor)
                    .Include(c => c.Inscripciones)
                    .FirstOrDefaultAsync(c => c.IdClase == id);

                if (clase == null)
                    return NotFound(ApiResponse<ClaseDto>.ErrorResponse("Clase no encontrada"));

                // Verificar permisos
                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<ClaseDto>.ErrorResponse("No autorizado"));

                return Ok(ApiResponse<ClaseDto>.SuccessResponse(_mappingService.ToDto(clase), "Clase obtenida"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClaseDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("{id}/estudiantes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PersonaSimpleDto>>>> GetEstudiantesByClase(int id)
        {
            try
            {
                var clase = await _context.Clases.FindAsync(id);
                if (clase == null)
                    return NotFound(ApiResponse<IEnumerable<PersonaSimpleDto>>.ErrorResponse("Clase no encontrada"));

                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<IEnumerable<PersonaSimpleDto>>.ErrorResponse("No autorizado"));

                var estudiantes = await _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Where(i => i.IdClase == id && i.Estado.ToLower() == "activa")
                    .Select(i => i.Estudiante)
                    .ToListAsync();

                var estudiantesDto = estudiantes.Select(e => _mappingService.ToSimpleDto(e));
                return Ok(ApiResponse<IEnumerable<PersonaSimpleDto>>.SuccessResponse(estudiantesDto, "Estudiantes obtenidos"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PersonaSimpleDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ClaseDto>>> PostClase(ClaseCreateDto claseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClaseDto>.ErrorResponse("Datos inválidos", ModelState));

            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<ClaseDto>.ErrorResponse("Solo administradores pueden crear clases"));

            try
            {
                // Verificar que el profesor existe y tiene rol profesor
                var profesor = await _context.Personas.FindAsync(claseDto.IdProfesor);
                if (profesor == null || profesor.Rol != "profesor")
                    return BadRequest(ApiResponse<ClaseDto>.ErrorResponse("El profesor especificado no existe o no tiene rol de profesor"));

                var clase = _mappingService.ToEntity(claseDto);
                _context.Clases.Add(clase);
                await _context.SaveChangesAsync();

                await _context.Entry(clase).Reference(c => c.EstiloDanza).LoadAsync();
                await _context.Entry(clase).Reference(c => c.Profesor).LoadAsync();

                return CreatedAtAction(nameof(GetClase), new { id = clase.IdClase },
                    ApiResponse<ClaseDto>.SuccessResponse(_mappingService.ToDto(clase), "Clase creada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClaseDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClaseDto>>> PutClase(int id, ClaseUpdateDto claseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClaseDto>.ErrorResponse("Datos inválidos", ModelState));

            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<ClaseDto>.ErrorResponse("Solo administradores pueden modificar clases"));

            try
            {
                var clase = await _context.Clases
                    .Include(c => c.EstiloDanza)
                    .Include(c => c.Profesor)
                    .Include(c => c.Inscripciones)
                    .FirstOrDefaultAsync(c => c.IdClase == id);

                if (clase == null)
                    return NotFound(ApiResponse<ClaseDto>.ErrorResponse("Clase no encontrada"));

                _mappingService.UpdateEntity(clase, claseDto);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<ClaseDto>.SuccessResponse(_mappingService.ToDto(clase), "Clase actualizada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClaseDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteClase(int id)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Solo administradores pueden eliminar clases"));

            try
            {
                var clase = await _context.Clases.FindAsync(id);
                if (clase == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Clase no encontrada"));

                clase.Activa = false;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Clase eliminada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("profesor/{profesorId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClaseDto>>>> GetClasesByProfesor(int profesorId)
        {
            try
            {
                var idPersona = ObtenerIdPersona();
                var esAdmin = EsAdministrador();
                
                // Debug: mostrar claims
                Console.WriteLine($"[DEBUG] profesorId solicitado: {profesorId}");
                Console.WriteLine($"[DEBUG] idPersona del token: {idPersona}");
                Console.WriteLine($"[DEBUG] esAdmin: {esAdmin}");
                Console.WriteLine($"[DEBUG] Claims disponibles:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  - {claim.Type}: {claim.Value}");
                }
                
                if (!esAdmin && profesorId != idPersona)
                    return StatusCode(403, ApiResponse<IEnumerable<ClaseDto>>.ErrorResponse($"No autorizado. profesorId={profesorId}, idPersona={idPersona}"));

                var clases = await _context.Clases
                    .Include(c => c.EstiloDanza)
                    .Include(c => c.Profesor)
                    .Include(c => c.Inscripciones)
                    .Where(c => c.IdProfesor == profesorId && c.Activa)
                    .ToListAsync();

                var clasesDto = clases.Select(c => _mappingService.ToDto(c));
                return Ok(ApiResponse<IEnumerable<ClaseDto>>.SuccessResponse(clasesDto, "Clases del profesor obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ClaseDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Reporte CSV de clases
        [HttpGet("reporte/csv")]
        public async Task<IActionResult> GetReporteCsv(
            [FromQuery] string? diaSemana = null,
            [FromQuery] int? idEstilo = null,
            [FromQuery] int? idProfesor = null,
            [FromQuery] bool? activa = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var clases = await ObtenerClasesParaReporte(diaSemana, idEstilo, idProfesor, activa);

                var csv = new System.Text.StringBuilder();
                csv.Append('\uFEFF'); // BOM para Excel
                csv.AppendLine("ID;Nombre Clase;Estilo;Profesor;Día;Hora;Duración (min);Capacidad;Inscritos;Cupos Libres;Precio Mensual;Estado");

                foreach (var c in clases)
                {
                    var profesor = c.Profesor != null ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}".Replace(";", " ") : "";
                    var estilo = c.EstiloDanza?.NombreEsti?.Replace(";", " ") ?? "";
                    var nombre = c.NombreClase?.Replace(";", " ") ?? "";
                    var inscritos = c.Inscripciones?.Count(i => i.Estado.ToLower() == "activa") ?? 0;
                    var cuposLibres = c.CapacidadMax - inscritos;
                    
                    csv.AppendLine($"{c.IdClase};{nombre};{estilo};{profesor};{c.DiaSemana};{c.Hora};{c.DuracionMinutos};{c.CapacidadMax};{inscritos};{cuposLibres};{c.PrecioMensuClas:F2};{(c.Activa ? "Activa" : "Inactiva")}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv; charset=utf-8", $"reporte_clases_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        // Reporte PDF de clases
        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GetReportePdf(
            [FromQuery] string? diaSemana = null,
            [FromQuery] int? idEstilo = null,
            [FromQuery] int? idProfesor = null,
            [FromQuery] bool? activa = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var clases = await ObtenerClasesParaReporte(diaSemana, idEstilo, idProfesor, activa);
                var html = GenerarHtmlReporteClases(clases);
                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html; charset=utf-8", $"reporte_clases_{DateTime.Now:yyyyMMdd}.html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        private async Task<List<Clase>> ObtenerClasesParaReporte(
            string? diaSemana, int? idEstilo, int? idProfesor, bool? activa)
        {
            var query = _context.Clases
                .Include(c => c.EstiloDanza)
                .Include(c => c.Profesor)
                .Include(c => c.Inscripciones)
                .AsQueryable();

            if (!string.IsNullOrEmpty(diaSemana))
                query = query.Where(c => c.DiaSemana == diaSemana);
            if (idEstilo.HasValue)
                query = query.Where(c => c.IdEstilo == idEstilo.Value);
            if (idProfesor.HasValue)
                query = query.Where(c => c.IdProfesor == idProfesor.Value);
            if (activa.HasValue)
                query = query.Where(c => c.Activa == activa.Value);

            return await query.OrderBy(c => c.DiaSemana).ThenBy(c => c.Hora).ToListAsync();
        }

        private string GenerarHtmlReporteClases(List<Clase> clases)
        {
            var totalCapacidad = clases.Sum(c => c.CapacidadMax);
            var totalInscritos = clases.Sum(c => c.Inscripciones?.Count(i => i.Estado.ToLower() == "activa") ?? 0);
            var activas = clases.Count(c => c.Activa);

            var rows = string.Join("", clases.Select(c => {
                var inscritos = c.Inscripciones?.Count(i => i.Estado.ToLower() == "activa") ?? 0;
                var cuposLibres = c.CapacidadMax - inscritos;
                return $@"
                <tr>
                    <td>{c.IdClase}</td>
                    <td>{c.NombreClase}</td>
                    <td>{c.EstiloDanza?.NombreEsti}</td>
                    <td>{c.Profesor?.Nombre} {c.Profesor?.Apellido}</td>
                    <td>{c.DiaSemana}</td>
                    <td>{c.Hora}</td>
                    <td>{c.DuracionMinutos} min</td>
                    <td>{c.CapacidadMax}</td>
                    <td>{inscritos}</td>
                    <td>{cuposLibres}</td>
                    <td>${c.PrecioMensuClas:F2}</td>
                    <td><span class='{(c.Activa ? "activa" : "inactiva")}'>{(c.Activa ? "Activa" : "Inactiva")}</span></td>
                </tr>";
            }));

            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Reporte de Clases</title>
<style>
    body {{ font-family: Arial, sans-serif; margin: 20px; }}
    h1 {{ color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }}
    .info {{ margin-bottom: 20px; color: #666; }}
    .stats {{ display: flex; gap: 20px; margin-bottom: 20px; }}
    .stat {{ background: #f5f5f5; padding: 15px; border-radius: 8px; text-align: center; min-width: 100px; }}
    .stat-number {{ font-size: 24px; font-weight: bold; color: #007bff; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 12px; }}
    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
    th {{ background-color: #007bff; color: white; }}
    tr:nth-child(even) {{ background-color: #f9f9f9; }}
    .activa {{ color: #28a745; font-weight: bold; }}
    .inactiva {{ color: #dc3545; font-weight: bold; }}
    @media print {{ body {{ margin: 0; }} table {{ font-size: 10px; }} }}
</style></head><body>
<h1>Reporte de Clases - Academia de Danza</h1>
<div class='info'>
    <p>Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
</div>
<div class='stats'>
    <div class='stat'><div class='stat-number'>{clases.Count}</div><div>Total Clases</div></div>
    <div class='stat'><div class='stat-number' style='color:#28a745'>{activas}</div><div>Activas</div></div>
    <div class='stat'><div class='stat-number'>{totalCapacidad}</div><div>Capacidad Total</div></div>
    <div class='stat'><div class='stat-number'>{totalInscritos}</div><div>Total Inscritos</div></div>
</div>
<table>
    <thead><tr>
        <th>ID</th><th>Clase</th><th>Estilo</th><th>Profesor</th><th>Día</th><th>Hora</th><th>Duración</th><th>Capacidad</th><th>Inscritos</th><th>Cupos</th><th>Precio</th><th>Estado</th>
    </tr></thead>
    <tbody>{rows}</tbody>
</table>
<script>window.onload = function() {{ window.print(); }}</script>
</body></html>";
        }
    }
}
