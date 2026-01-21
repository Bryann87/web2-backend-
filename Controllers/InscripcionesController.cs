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
    public class InscripcionesController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;

        public InscripcionesController(AcademiaContext context, IMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
        }

        private bool EsAdministrador() => User.FindFirst("rol")?.Value == "administrador";
        private int? ObtenerIdPersona() => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<InscripcionDto>>>> GetInscripciones(
            [FromQuery] PaginationParams pagination,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idClase = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                var query = _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Include(i => i.Clase)
                        .ThenInclude(c => c!.EstiloDanza)
                    .AsQueryable();

                // Profesores solo ven inscripciones de sus clases
                if (!EsAdministrador())
                {
                    var idPersona = ObtenerIdPersona();
                    query = query.Where(i => i.Clase!.IdProfesor == idPersona);
                }

                if (idEstudiante.HasValue)
                    query = query.Where(i => i.IdEstudiante == idEstudiante.Value);
                if (idClase.HasValue)
                    query = query.Where(i => i.IdClase == idClase.Value);
                if (!string.IsNullOrEmpty(estado))
                    query = query.Where(i => i.Estado.ToLower() == estado.ToLower());

                var totalRecords = await query.CountAsync();
                var inscripciones = await query
                    .OrderByDescending(i => i.FechaInsc)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var inscripcionesDto = inscripciones.Select(i => _mappingService.ToDto(i));
                var paginatedResponse = new PaginatedResponse<InscripcionDto>
                {
                    Data = inscripcionesDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<InscripcionDto>>.SuccessResponse(paginatedResponse, "Inscripciones obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<InscripcionDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InscripcionDto>>> GetInscripcion(int id)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<InscripcionDto>.ErrorResponse("Solo administradores"));

            try
            {
                var inscripcion = await _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Include(i => i.Clase)
                        .ThenInclude(c => c!.EstiloDanza)
                    .FirstOrDefaultAsync(i => i.IdInsc == id);

                if (inscripcion == null)
                    return NotFound(ApiResponse<InscripcionDto>.ErrorResponse("Inscripción no encontrada"));

                return Ok(ApiResponse<InscripcionDto>.SuccessResponse(_mappingService.ToDto(inscripcion), "Inscripción obtenida"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InscripcionDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("clase/{claseId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InscripcionDto>>>> GetInscripcionesByClase(int claseId)
        {
            try
            {
                // Verificar que la clase existe
                var clase = await _context.Clases.FindAsync(claseId);
                if (clase == null)
                    return NotFound(ApiResponse<IEnumerable<InscripcionDto>>.ErrorResponse("Clase no encontrada"));

                // Profesores solo pueden ver inscripciones de sus propias clases
                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<IEnumerable<InscripcionDto>>.ErrorResponse("No autorizado"));

                var inscripciones = await _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Include(i => i.Clase)
                    .Where(i => i.IdClase == claseId && i.Estado.ToLower() == "activa")
                    .ToListAsync();

                var inscripcionesDto = inscripciones.Select(i => _mappingService.ToDto(i));
                return Ok(ApiResponse<IEnumerable<InscripcionDto>>.SuccessResponse(inscripcionesDto, "Inscripciones obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<InscripcionDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("estudiante/{estudianteId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InscripcionDto>>>> GetInscripcionesByEstudiante(int estudianteId)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<IEnumerable<InscripcionDto>>.ErrorResponse("Solo administradores"));

            try
            {
                var inscripciones = await _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Include(i => i.Clase)
                        .ThenInclude(c => c!.EstiloDanza)
                    .Where(i => i.IdEstudiante == estudianteId)
                    .ToListAsync();

                var inscripcionesDto = inscripciones.Select(i => _mappingService.ToDto(i));
                return Ok(ApiResponse<IEnumerable<InscripcionDto>>.SuccessResponse(inscripcionesDto, "Inscripciones obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<InscripcionDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InscripcionDto>>> PostInscripcion(InscripcionCreateDto inscripcionDto)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<InscripcionDto>.ErrorResponse("Solo administradores"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("Datos inválidos", ModelState));

            try
            {
                // Verificar estudiante
                var estudiante = await _context.Personas.FindAsync(inscripcionDto.IdEstudiante);
                if (estudiante == null)
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("Estudiante no encontrado"));
                
                if (estudiante.Rol?.ToLower() != "estudiante")
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("La persona seleccionada no es un estudiante"));

                // Verificar clase
                var clase = await _context.Clases
                    .Include(c => c.EstiloDanza)
                    .FirstOrDefaultAsync(c => c.IdClase == inscripcionDto.IdClase);
                    
                if (clase == null)
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("Clase no encontrada"));

                if (!clase.Activa)
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("La clase no está activa. No se pueden crear nuevas inscripciones"));

                // Verificar si ya está inscrito en esta clase (activa)
                var inscripcionExistente = await _context.Inscripciones
                    .FirstOrDefaultAsync(i => i.IdEstudiante == inscripcionDto.IdEstudiante && 
                                             i.IdClase == inscripcionDto.IdClase && 
                                             i.Estado.ToLower() == "activa");
                                             
                if (inscripcionExistente != null)
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse(
                        $"El estudiante {estudiante.Nombre} {estudiante.Apellido} ya está inscrito en la clase {clase.NombreClase}"));

                // Verificar cupo disponible en la clase
                var inscripcionesActivas = await _context.Inscripciones
                    .CountAsync(i => i.IdClase == inscripcionDto.IdClase && i.Estado.ToLower() == "activa");
                    
                if (clase.CapacidadMax > 0 && inscripcionesActivas >= clase.CapacidadMax)
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse(
                        $"La clase {clase.NombreClase} ha alcanzado su cupo máximo ({clase.CapacidadMax} estudiantes)"));

                // Verificar si el estudiante tiene inscripciones activas en el mismo horario
                var inscripcionesEstudiante = await _context.Inscripciones
                    .Include(i => i.Clase)
                    .Where(i => i.IdEstudiante == inscripcionDto.IdEstudiante && 
                               i.Estado.ToLower() == "activa")
                    .ToListAsync();

                var conflictoHorario = inscripcionesEstudiante.Any(i => 
                    i.Clase != null && 
                    i.Clase.DiaSemana?.ToLower() == clase.DiaSemana?.ToLower() && 
                    i.Clase.Hora == clase.Hora);

                if (conflictoHorario)
                {
                    var claseConflicto = inscripcionesEstudiante
                        .First(i => i.Clase != null && 
                                   i.Clase.DiaSemana?.ToLower() == clase.DiaSemana?.ToLower() && 
                                   i.Clase.Hora == clase.Hora)
                        .Clase;
                    return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse(
                        $"El estudiante ya tiene una clase inscrita el {clase.DiaSemana} a las {clase.Hora} ({claseConflicto?.NombreClase})"));
                }

                var inscripcion = _mappingService.ToEntity(inscripcionDto);
                _context.Inscripciones.Add(inscripcion);
                await _context.SaveChangesAsync();

                await _context.Entry(inscripcion).Reference(i => i.Estudiante).LoadAsync();
                await _context.Entry(inscripcion).Reference(i => i.Clase).LoadAsync();
                if (inscripcion.Clase != null)
                {
                    await _context.Entry(inscripcion.Clase).Reference(c => c.EstiloDanza).LoadAsync();
                }

                return CreatedAtAction(nameof(GetInscripcion), new { id = inscripcion.IdInsc },
                    ApiResponse<InscripcionDto>.SuccessResponse(_mappingService.ToDto(inscripcion), 
                        $"Inscripción creada exitosamente. {estudiante.Nombre} {estudiante.Apellido} inscrito en {clase.NombreClase}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InscripcionDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InscripcionDto>>> PutInscripcion(int id, InscripcionUpdateDto inscripcionDto)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<InscripcionDto>.ErrorResponse("Solo administradores"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InscripcionDto>.ErrorResponse("Datos inválidos", ModelState));

            try
            {
                var inscripcion = await _context.Inscripciones
                    .Include(i => i.Estudiante)
                    .Include(i => i.Clase)
                    .FirstOrDefaultAsync(i => i.IdInsc == id);

                if (inscripcion == null)
                    return NotFound(ApiResponse<InscripcionDto>.ErrorResponse("Inscripción no encontrada"));

                _mappingService.UpdateEntity(inscripcion, inscripcionDto);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<InscripcionDto>.SuccessResponse(_mappingService.ToDto(inscripcion), "Inscripción actualizada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InscripcionDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteInscripcion(int id)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Solo administradores"));

            try
            {
                var inscripcion = await _context.Inscripciones.FindAsync(id);
                if (inscripcion == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Inscripción no encontrada"));

                // Soft delete - cambiar estado a inactiva
                inscripcion.Estado = "Inactiva";
                inscripcion.FechaBaja = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Inscripción eliminada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Reporte CSV de inscripciones
        [HttpGet("reporte/csv")]
        public async Task<IActionResult> GetReporteCsv(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idClase = null,
            [FromQuery] string? estado = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var inscripciones = await ObtenerInscripcionesParaReporte(fechaInicio, fechaFin, idClase, estado);

                var csv = new System.Text.StringBuilder();
                csv.Append('\uFEFF'); // BOM para Excel
                csv.AppendLine("ID;Fecha Inscripción;Estudiante;Cédula;Clase;Estilo;Día;Hora;Estado;Precio Mensual");

                foreach (var i in inscripciones)
                {
                    var estudiante = $"{i.Estudiante?.Nombre} {i.Estudiante?.Apellido}".Replace(";", " ");
                    var cedula = i.Estudiante?.Cedula?.Replace(";", " ") ?? "";
                    var clase = i.Clase?.NombreClase?.Replace(";", " ") ?? "";
                    var estilo = i.Clase?.EstiloDanza?.NombreEsti?.Replace(";", " ") ?? "";
                    
                    csv.AppendLine($"{i.IdInsc};{i.FechaInsc:yyyy-MM-dd};{estudiante};{cedula};{clase};{estilo};{i.Clase?.DiaSemana};{i.Clase?.Hora};{i.Estado};{i.Clase?.PrecioMensuClas:F2}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv; charset=utf-8", $"reporte_inscripciones_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        // Reporte PDF de inscripciones
        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GetReportePdf(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idClase = null,
            [FromQuery] string? estado = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var inscripciones = await ObtenerInscripcionesParaReporte(fechaInicio, fechaFin, idClase, estado);
                var html = GenerarHtmlReporteInscripciones(inscripciones, fechaInicio, fechaFin);
                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html; charset=utf-8", $"reporte_inscripciones_{DateTime.Now:yyyyMMdd}.html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        private async Task<List<Inscripcion>> ObtenerInscripcionesParaReporte(
            DateTime? fechaInicio, DateTime? fechaFin, int? idClase, string? estado)
        {
            var query = _context.Inscripciones
                .Include(i => i.Estudiante)
                .Include(i => i.Clase)
                    .ThenInclude(c => c!.EstiloDanza)
                .AsQueryable();

            // Filtros de fecha - las fechas vienen en UTC desde el frontend
            if (fechaInicio.HasValue)
            {
                var fechaInicioUtc = DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc);
                query = query.Where(i => i.FechaInsc >= fechaInicioUtc);
            }
            if (fechaFin.HasValue)
            {
                var fechaFinUtc = DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc);
                query = query.Where(i => i.FechaInsc <= fechaFinUtc);
            }
            if (idClase.HasValue)
                query = query.Where(i => i.IdClase == idClase.Value);
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(i => i.Estado.ToLower() == estado.ToLower());

            return await query.OrderByDescending(i => i.FechaInsc).ToListAsync();
        }

        private string GenerarHtmlReporteInscripciones(List<Inscripcion> inscripciones, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            else if (fechaInicio.HasValue)
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            else if (fechaFin.HasValue)
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";

            var activas = inscripciones.Count(i => i.Estado == "Activa");
            var suspendidas = inscripciones.Count(i => i.Estado == "Suspendida");
            var canceladas = inscripciones.Count(i => i.Estado == "Cancelada");

            var rows = string.Join("", inscripciones.Select(i => $@"
                <tr>
                    <td>{i.IdInsc}</td>
                    <td>{i.FechaInsc:dd/MM/yyyy}</td>
                    <td>{i.Estudiante?.Nombre} {i.Estudiante?.Apellido}</td>
                    <td>{i.Estudiante?.Cedula}</td>
                    <td>{i.Clase?.NombreClase}</td>
                    <td>{i.Clase?.EstiloDanza?.NombreEsti}</td>
                    <td>{i.Clase?.DiaSemana} {i.Clase?.Hora}</td>
                    <td><span class='estado-{i.Estado?.ToLower()}'>{i.Estado}</span></td>
                </tr>"));

            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Reporte de Inscripciones</title>
<style>
    body {{ font-family: Arial, sans-serif; margin: 20px; }}
    h1 {{ color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }}
    .info {{ margin-bottom: 20px; color: #666; }}
    .stats {{ display: flex; gap: 20px; margin-bottom: 20px; }}
    .stat {{ background: #f5f5f5; padding: 15px; border-radius: 8px; text-align: center; }}
    .stat-number {{ font-size: 24px; font-weight: bold; color: #007bff; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
    th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
    th {{ background-color: #007bff; color: white; }}
    tr:nth-child(even) {{ background-color: #f9f9f9; }}
    .estado-activa {{ color: #28a745; font-weight: bold; }}
    .estado-suspendida {{ color: #ffc107; font-weight: bold; }}
    .estado-cancelada {{ color: #dc3545; font-weight: bold; }}
    @media print {{ body {{ margin: 0; }} }}
</style></head><body>
<h1>Reporte de Inscripciones - Academia de Danza</h1>
<div class='info'>
    <p>Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    <p>{periodo}</p>
</div>
<div class='stats'>
    <div class='stat'><div class='stat-number'>{inscripciones.Count}</div><div>Total</div></div>
    <div class='stat'><div class='stat-number' style='color:#28a745'>{activas}</div><div>Activas</div></div>
    <div class='stat'><div class='stat-number' style='color:#ffc107'>{suspendidas}</div><div>Suspendidas</div></div>
    <div class='stat'><div class='stat-number' style='color:#dc3545'>{canceladas}</div><div>Canceladas</div></div>
</div>
<table>
    <thead><tr>
        <th>ID</th><th>Fecha</th><th>Estudiante</th><th>Cédula</th><th>Clase</th><th>Estilo</th><th>Horario</th><th>Estado</th>
    </tr></thead>
    <tbody>{rows}</tbody>
</table>
<script>window.onload = function() {{ window.print(); }}</script>
</body></html>";
        }
    }
}
