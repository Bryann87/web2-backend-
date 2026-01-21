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
    public class AsistenciasController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;
        private readonly INotificacionService _notificacionService;

        public AsistenciasController(AcademiaContext context, IMappingService mappingService, INotificacionService notificacionService)
        {
            _context = context;
            _mappingService = mappingService;
            _notificacionService = notificacionService;
        }

        private bool EsAdministrador() => User.FindFirst("rol")?.Value == "administrador";
        private int? ObtenerIdPersona()
        {
            // Intentar obtener el ID desde diferentes claims posibles
            var subClaim = User.FindFirst("sub")?.Value;
            var nameidClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var idStr = subClaim ?? nameidClaim;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AsistenciaDto>>>> Get(
            [FromQuery] PaginationParams pagination,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idClase = null,
            [FromQuery] int? idInscripcion = null,
            [FromQuery] string? estadoAsis = null)
        {
            try
            {
                IQueryable<Asistencia> query = _context.Asistencias
                    .Include(a => a.Estudiante)
                    .Include(a => a.Clase)
                        .ThenInclude(c => c!.EstiloDanza);

                // Profesores solo ven asistencias de sus clases
                if (!EsAdministrador())
                {
                    var idPersona = ObtenerIdPersona();
                    query = query.Where(a => a.Clase!.IdProfesor == idPersona);
                }

                // Filtros de fecha - las fechas vienen en UTC desde el frontend
                if (fechaInicio.HasValue)
                {
                    var fechaInicioUtc = DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc);
                    query = query.Where(a => a.FechaAsis >= fechaInicioUtc);
                }
                if (fechaFin.HasValue)
                {
                    var fechaFinUtc = DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc);
                    query = query.Where(a => a.FechaAsis <= fechaFinUtc);
                }
                if (idEstudiante.HasValue)
                    query = query.Where(a => a.IdEstudiante == idEstudiante.Value);
                if (idClase.HasValue)
                    query = query.Where(a => a.IdClase == idClase.Value);
                if (idInscripcion.HasValue)
                {
                    // Obtener la inscripción para filtrar por estudiante y clase
                    var inscripcion = await _context.Inscripciones.FindAsync(idInscripcion.Value);
                    if (inscripcion != null)
                    {
                        query = query.Where(a => a.IdEstudiante == inscripcion.IdEstudiante && a.IdClase == inscripcion.IdClase);
                    }
                }
                if (!string.IsNullOrEmpty(estadoAsis))
                    query = query.Where(a => a.EstadoAsis.ToLower() == estadoAsis.ToLower());

                var totalRecords = await query.CountAsync();
                var asistencias = await query
                    .OrderByDescending(a => a.FechaAsis)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var asistenciasDto = asistencias.Select(a => _mappingService.ToDto(a));
                var paginatedResponse = new PaginatedResponse<AsistenciaDto>
                {
                    Data = asistenciasDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<AsistenciaDto>>.SuccessResponse(paginatedResponse, "Asistencias obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<AsistenciaDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AsistenciaDto>>> Get(int id)
        {
            try
            {
                var asistencia = await _context.Asistencias
                    .Include(a => a.Estudiante)
                    .Include(a => a.Clase)
                    .FirstOrDefaultAsync(a => a.IdAsist == id);

                if (asistencia == null)
                    return NotFound(ApiResponse<AsistenciaDto>.ErrorResponse("Asistencia no encontrada"));

                return Ok(ApiResponse<AsistenciaDto>.SuccessResponse(_mappingService.ToDto(asistencia), "Asistencia obtenida"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AsistenciaDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("clase/{claseId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AsistenciaDto>>>> GetAsistenciasByClase(int claseId, [FromQuery] DateTime? fecha = null)
        {
            try
            {
                var clase = await _context.Clases.FindAsync(claseId);
                if (clase == null)
                    return NotFound(ApiResponse<IEnumerable<AsistenciaDto>>.ErrorResponse("Clase no encontrada"));

                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<IEnumerable<AsistenciaDto>>.ErrorResponse("No autorizado"));

                var query = _context.Asistencias
                    .Include(a => a.Estudiante)
                    .Include(a => a.Clase)
                    .Where(a => a.IdClase == claseId);

                if (fecha.HasValue)
                {
                    var fechaInicio = DateTime.SpecifyKind(fecha.Value.Date, DateTimeKind.Utc);
                    var fechaFin = fechaInicio.AddDays(1);
                    query = query.Where(a => a.FechaAsis >= fechaInicio && a.FechaAsis < fechaFin);
                }

                var asistencias = await query.OrderByDescending(a => a.FechaAsis).ToListAsync();
                var asistenciasDto = asistencias.Select(a => _mappingService.ToDto(a));

                return Ok(ApiResponse<IEnumerable<AsistenciaDto>>.SuccessResponse(asistenciasDto, "Asistencias obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AsistenciaDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("estudiante/{estudianteId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AsistenciaDto>>>> GetAsistenciasByEstudiante(int estudianteId)
        {
            try
            {
                var asistencias = await _context.Asistencias
                    .Include(a => a.Estudiante)
                    .Include(a => a.Clase)
                    .Where(a => a.IdEstudiante == estudianteId)
                    .OrderByDescending(a => a.FechaAsis)
                    .ToListAsync();

                var asistenciasDto = asistencias.Select(a => _mappingService.ToDto(a));
                return Ok(ApiResponse<IEnumerable<AsistenciaDto>>.SuccessResponse(asistenciasDto, "Asistencias obtenidas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AsistenciaDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("clase/{claseId}/validar")]
        public async Task<ActionResult<ApiResponse<ValidacionAsistenciaDto>>> ValidarAsistencia(int claseId)
        {
            try
            {
                var clase = await _context.Clases.FindAsync(claseId);
                if (clase == null)
                    return NotFound(ApiResponse<ValidacionAsistenciaDto>.ErrorResponse("Clase no encontrada"));

                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<ValidacionAsistenciaDto>.ErrorResponse("No autorizado"));

                var validacion = ValidarDiaClase(clase);
                return Ok(ApiResponse<ValidacionAsistenciaDto>.SuccessResponse(validacion, "Validación completada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ValidacionAsistenciaDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Zona horaria de Ecuador (UTC-5)
        // En Windows el ID es "SA Pacific Standard Time", en Linux/Mac es "America/Guayaquil"
        private static readonly TimeZoneInfo EcuadorTimeZone = GetEcuadorTimeZone();
        
        private static TimeZoneInfo GetEcuadorTimeZone()
        {
            try
            {
                // Intentar primero con el ID de IANA (Linux/Mac/Docker)
                return TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");
            }
            catch (TimeZoneNotFoundException)
            {
                // Si falla, usar el ID de Windows
                return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            }
        }
        
        private static DateTime GetEcuadorNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EcuadorTimeZone);

        private ValidacionAsistenciaDto ValidarDiaClase(Clase clase)
        {
            var ahoraEcuador = GetEcuadorNow();
            var diaActual = ahoraEcuador.DayOfWeek;
            var diaSemanaClase = clase.DiaSemana.ToLower().Trim();
            
            // Mapeo de días en español a DayOfWeek
            var diasMap = new Dictionary<string, DayOfWeek>(StringComparer.OrdinalIgnoreCase)
            {
                { "lunes", DayOfWeek.Monday },
                { "martes", DayOfWeek.Tuesday },
                { "miércoles", DayOfWeek.Wednesday },
                { "miercoles", DayOfWeek.Wednesday },
                { "jueves", DayOfWeek.Thursday },
                { "viernes", DayOfWeek.Friday },
                { "sábado", DayOfWeek.Saturday },
                { "sabado", DayOfWeek.Saturday },
                { "domingo", DayOfWeek.Sunday }
            };

            var diasNombres = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday, "Lunes" },
                { DayOfWeek.Tuesday, "Martes" },
                { DayOfWeek.Wednesday, "Miércoles" },
                { DayOfWeek.Thursday, "Jueves" },
                { DayOfWeek.Friday, "Viernes" },
                { DayOfWeek.Saturday, "Sábado" },
                { DayOfWeek.Sunday, "Domingo" }
            };

            if (!diasMap.TryGetValue(diaSemanaClase, out var diaClase))
            {
                return new ValidacionAsistenciaDto
                {
                    PuedeRegistrar = true,
                    Mensaje = "Día de clase no reconocido, se permite el registro",
                    DiaSemanaClase = clase.DiaSemana,
                    DiaActual = diasNombres[diaActual],
                    HoraServidorUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    HoraServidorEcuador = ahoraEcuador.ToString("yyyy-MM-dd HH:mm:ss"),
                    ZonaHorariaUsada = EcuadorTimeZone.Id
                };
            }

            var puedeRegistrar = diaActual == diaClase;
            var diasHastaProxima = ((int)diaClase - (int)diaActual + 7) % 7;
            if (diasHastaProxima == 0 && !puedeRegistrar) diasHastaProxima = 7;
            var proximaFecha = ahoraEcuador.AddDays(diasHastaProxima);

            return new ValidacionAsistenciaDto
            {
                PuedeRegistrar = puedeRegistrar,
                Mensaje = puedeRegistrar 
                    ? "Puede registrar asistencia" 
                    : $"No se puede registrar asistencia. Hoy es {diasNombres[diaActual]} y la clase es los días {clase.DiaSemana}.",
                DiaSemanaClase = clase.DiaSemana,
                DiaActual = diasNombres[diaActual],
                ProximaFechaDisponible = puedeRegistrar ? null : proximaFecha.ToString("yyyy-MM-dd"),
                HoraServidorUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                HoraServidorEcuador = ahoraEcuador.ToString("yyyy-MM-dd HH:mm:ss"),
                ZonaHorariaUsada = EcuadorTimeZone.Id
            };
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AsistenciaDto>>> Post(AsistenciaCreateDto asistenciaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AsistenciaDto>.ErrorResponse("Datos inválidos", ModelState));

            try
            {
                // Verificar que la clase existe y el usuario tiene permisos
                var clase = await _context.Clases.FindAsync(asistenciaDto.IdClase);
                if (clase == null)
                    return BadRequest(ApiResponse<AsistenciaDto>.ErrorResponse("Clase no encontrada"));

                if (!EsAdministrador() && clase.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<AsistenciaDto>.ErrorResponse("No autorizado"));

                // Validar que sea el día correcto de la clase
                var validacion = ValidarDiaClase(clase);
                if (!validacion.PuedeRegistrar)
                    return BadRequest(ApiResponse<AsistenciaDto>.ErrorResponse(validacion.Mensaje));

                // Verificar que el estudiante existe y tiene rol estudiante
                var estudiante = await _context.Personas.FindAsync(asistenciaDto.IdEstudiante);
                if (estudiante == null || estudiante.Rol != "estudiante")
                    return BadRequest(ApiResponse<AsistenciaDto>.ErrorResponse("Estudiante no encontrado"));

                var asistencia = _mappingService.ToEntity(asistenciaDto);
                asistencia.RegistradaPor = ObtenerIdPersona();
                
                _context.Asistencias.Add(asistencia);
                await _context.SaveChangesAsync();

                await _context.Entry(asistencia).Reference(a => a.Estudiante).LoadAsync();
                await _context.Entry(asistencia).Reference(a => a.Clase).LoadAsync();

                // Obtener el nombre del usuario que registra la asistencia
                var registradoPor = await _context.Personas.FindAsync(ObtenerIdPersona());
                var nombreRegistrador = registradoPor != null 
                    ? $"{registradoPor.Nombre} {registradoPor.Apellido}" 
                    : "Usuario desconocido";

                var responseDto = _mappingService.ToDto(asistencia);
                await _notificacionService.NotificarNuevaAsistencia(asistencia.IdClase, responseDto, nombreRegistrador);

                return CreatedAtAction(nameof(Get), new { id = asistencia.IdAsist },
                    ApiResponse<AsistenciaDto>.SuccessResponse(responseDto, "Asistencia registrada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AsistenciaDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<AsistenciaDto>>> Put(int id, AsistenciaUpdateDto asistenciaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AsistenciaDto>.ErrorResponse("Datos inválidos", ModelState));

            try
            {
                var asistencia = await _context.Asistencias
                    .Include(a => a.Estudiante)
                    .Include(a => a.Clase)
                    .FirstOrDefaultAsync(a => a.IdAsist == id);

                if (asistencia == null)
                    return NotFound(ApiResponse<AsistenciaDto>.ErrorResponse("Asistencia no encontrada"));

                if (!EsAdministrador() && asistencia.Clase?.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<AsistenciaDto>.ErrorResponse("No autorizado"));

                _mappingService.UpdateEntity(asistencia, asistenciaDto);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<AsistenciaDto>.SuccessResponse(_mappingService.ToDto(asistencia), "Asistencia actualizada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AsistenciaDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object?>>> Delete(int id)
        {
            try
            {
                var asistencia = await _context.Asistencias
                    .Include(a => a.Clase)
                    .FirstOrDefaultAsync(a => a.IdAsist == id);
                    
                if (asistencia == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Asistencia no encontrada"));

                // Profesores pueden eliminar asistencias de sus propias clases
                if (!EsAdministrador() && asistencia.Clase?.IdProfesor != ObtenerIdPersona())
                    return StatusCode(403, ApiResponse<object>.ErrorResponse("No autorizado"));

                _context.Asistencias.Remove(asistencia);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Asistencia eliminada"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("reporte/csv")]
        public async Task<IActionResult> GetReporteCsv(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idClase = null,
            [FromQuery] int? idInscripcion = null,
            [FromQuery] string? estadoAsis = null)
        {
            try
            {
                var asistencias = await ObtenerAsistenciasParaReporte(fechaInicio, fechaFin, idEstudiante, idClase, idInscripcion, estadoAsis);

                var csv = new System.Text.StringBuilder();
                // BOM para Excel y separador punto y coma para mejor compatibilidad
                csv.Append('\uFEFF');
                csv.AppendLine("ID;Fecha;Estudiante;Cédula;Clase;Estilo;Día;Estado;Observaciones");

                foreach (var a in asistencias)
                {
                    var estudiante = $"{a.Estudiante?.Nombre} {a.Estudiante?.Apellido}".Replace(";", " ");
                    var cedula = a.Estudiante?.Cedula?.Replace(";", " ") ?? "";
                    var clase = a.Clase?.NombreClase?.Replace(";", " ") ?? "";
                    var estilo = a.Clase?.EstiloDanza?.NombreEsti?.Replace(";", " ") ?? "";
                    var dia = a.Clase?.DiaSemana ?? "";
                    var observaciones = a.Observaciones?.Replace(";", " ").Replace("\n", " ") ?? "";
                    
                    csv.AppendLine($"{a.IdAsist};{a.FechaAsis:yyyy-MM-dd};{estudiante};{cedula};{clase};{estilo};{dia};{a.EstadoAsis};{observaciones}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv; charset=utf-8", $"reporte_asistencias_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al generar reporte", ex.Message));
            }
        }

        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GetReportePdf(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idClase = null,
            [FromQuery] int? idInscripcion = null,
            [FromQuery] string? estadoAsis = null)
        {
            try
            {
                var asistencias = await ObtenerAsistenciasParaReporte(fechaInicio, fechaFin, idEstudiante, idClase, idInscripcion, estadoAsis);
                var html = GenerarHtmlReporteAsistencias(asistencias, fechaInicio, fechaFin);
                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html; charset=utf-8", $"reporte_asistencias_{DateTime.Now:yyyyMMdd}.html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al generar reporte", ex.Message));
            }
        }

        private string GenerarHtmlReporteAsistencias(List<Asistencia> asistencias, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            else if (fechaInicio.HasValue)
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            else if (fechaFin.HasValue)
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";

            var presentes = asistencias.Count(a => a.EstadoAsis.ToLower() == "presente");
            var ausentes = asistencias.Count(a => a.EstadoAsis.ToLower() == "ausente");
            var tardanzas = asistencias.Count(a => a.EstadoAsis.ToLower() == "tardanza");
            var porcentaje = asistencias.Count > 0 ? (presentes * 100.0 / asistencias.Count) : 0;

            var rows = string.Join("", asistencias.Select(a => $@"
                <tr>
                    <td>{a.IdAsist}</td>
                    <td>{a.FechaAsis:dd/MM/yyyy}</td>
                    <td>{a.Estudiante?.Nombre} {a.Estudiante?.Apellido}</td>
                    <td>{a.Clase?.NombreClase}</td>
                    <td>{a.Clase?.DiaSemana}</td>
                    <td><span class='estado-{a.EstadoAsis.ToLower()}'>{a.EstadoAsis}</span></td>
                </tr>"));

            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Reporte de Asistencias</title>
<style>
    body {{ font-family: Arial, sans-serif; margin: 20px; }}
    h1 {{ color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }}
    .info {{ margin-bottom: 20px; color: #666; }}
    .stats {{ display: flex; gap: 20px; margin-bottom: 20px; }}
    .stat {{ background: #f5f5f5; padding: 15px; border-radius: 8px; text-align: center; min-width: 100px; }}
    .stat-number {{ font-size: 24px; font-weight: bold; color: #007bff; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
    th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
    th {{ background-color: #007bff; color: white; }}
    tr:nth-child(even) {{ background-color: #f9f9f9; }}
    .estado-presente {{ color: #28a745; font-weight: bold; }}
    .estado-ausente {{ color: #dc3545; font-weight: bold; }}
    .estado-tardanza {{ color: #ffc107; font-weight: bold; }}
    .estado-justificado {{ color: #17a2b8; font-weight: bold; }}
    @media print {{ body {{ margin: 0; }} }}
</style></head><body>
<h1>Reporte de Asistencias - Academia de Danza</h1>
<div class='info'>
    <p>Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    <p>{periodo}</p>
</div>
<div class='stats'>
    <div class='stat'><div class='stat-number'>{asistencias.Count}</div><div>Total</div></div>
    <div class='stat'><div class='stat-number' style='color:#28a745'>{presentes}</div><div>Presentes</div></div>
    <div class='stat'><div class='stat-number' style='color:#dc3545'>{ausentes}</div><div>Ausentes</div></div>
    <div class='stat'><div class='stat-number' style='color:#ffc107'>{tardanzas}</div><div>Tardanzas</div></div>
    <div class='stat'><div class='stat-number'>{porcentaje:F1}%</div><div>Asistencia</div></div>
</div>
<table>
    <thead><tr>
        <th>ID</th><th>Fecha</th><th>Estudiante</th><th>Clase</th><th>Día</th><th>Estado</th>
    </tr></thead>
    <tbody>{rows}</tbody>
</table>
<script>window.onload = function() {{ window.print(); }}</script>
</body></html>";
        }

        [HttpGet("reporte/json")]
        public async Task<IActionResult> GetReporteJson(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] int? idClase = null,
            [FromQuery] int? idInscripcion = null,
            [FromQuery] string? estadoAsis = null)
        {
            try
            {
                var asistencias = await ObtenerAsistenciasParaReporte(fechaInicio, fechaFin, idEstudiante, idClase, idInscripcion, estadoAsis);
                var asistenciasDto = asistencias.Select(a => _mappingService.ToDto(a));

                var json = System.Text.Json.JsonSerializer.Serialize(asistenciasDto, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", $"reporte_asistencias_{DateTime.Now:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al generar reporte", ex.Message));
            }
        }

        private async Task<List<Asistencia>> ObtenerAsistenciasParaReporte(
            DateTime? fechaInicio, DateTime? fechaFin, int? idEstudiante, int? idClase, int? idInscripcion, string? estadoAsis)
        {
            IQueryable<Asistencia> query = _context.Asistencias
                .Include(a => a.Estudiante)
                .Include(a => a.Clase)
                    .ThenInclude(c => c!.EstiloDanza);

            if (!EsAdministrador())
            {
                var idPersona = ObtenerIdPersona();
                query = query.Where(a => a.Clase!.IdProfesor == idPersona);
            }

            // Filtros de fecha - las fechas vienen en UTC desde el frontend
            if (fechaInicio.HasValue)
            {
                var fechaInicioUtc = DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc);
                query = query.Where(a => a.FechaAsis >= fechaInicioUtc);
            }
            if (fechaFin.HasValue)
            {
                var fechaFinUtc = DateTime.SpecifyKind(fechaFin.Value, DateTimeKind.Utc);
                query = query.Where(a => a.FechaAsis <= fechaFinUtc);
            }
            if (idEstudiante.HasValue)
                query = query.Where(a => a.IdEstudiante == idEstudiante.Value);
            if (idClase.HasValue)
                query = query.Where(a => a.IdClase == idClase.Value);
            if (idInscripcion.HasValue)
            {
                // Obtener la inscripción para filtrar por estudiante y clase
                var inscripcion = await _context.Inscripciones.FindAsync(idInscripcion.Value);
                if (inscripcion != null)
                {
                    query = query.Where(a => a.IdEstudiante == inscripcion.IdEstudiante && a.IdClase == inscripcion.IdClase);
                }
            }
            if (!string.IsNullOrEmpty(estadoAsis))
                query = query.Where(a => a.EstadoAsis.ToLower() == estadoAsis.ToLower());

            return await query.OrderByDescending(a => a.FechaAsis).ToListAsync();
        }
    }
}
