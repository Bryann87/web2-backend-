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
    public class CobrosController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;
        private readonly INotificacionService _notificacionService;

        public CobrosController(AcademiaContext context, IMappingService mappingService, INotificacionService notificacionService)
        {
            _context = context;
            _mappingService = mappingService;
            _notificacionService = notificacionService;
        }

        private bool EsAdministrador() => User.FindFirst("rol")?.Value == "administrador";

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CobroDto>>>> GetCobros(
            [FromQuery] PaginationParams pagination,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] string? estadoCobro = null,
            [FromQuery] string? busqueda = null,
            [FromQuery] string? tipoCobro = null,
            [FromQuery] string? metodoPago = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<PaginatedResponse<CobroDto>>.ErrorResponse("Solo administradores pueden ver cobros"));

            try
            {
                var query = _context.Cobros.Include(c => c.Estudiante).AsQueryable();

                if (idEstudiante.HasValue)
                    query = query.Where(c => c.IdEstudiante == idEstudiante.Value);
                if (!string.IsNullOrEmpty(estadoCobro))
                    query = query.Where(c => c.EstadoCobro == estadoCobro);
                if (!string.IsNullOrEmpty(tipoCobro))
                    query = query.Where(c => c.TipoCobro == tipoCobro);
                if (!string.IsNullOrEmpty(metodoPago))
                    query = query.Where(c => c.MetodoPago == metodoPago);
                
                // Filtros de fecha
                if (fechaInicio.HasValue)
                    query = query.Where(c => c.FechaPago >= DateTime.SpecifyKind(fechaInicio.Value.Date, DateTimeKind.Utc));
                if (fechaFin.HasValue)
                    query = query.Where(c => c.FechaPago < DateTime.SpecifyKind(fechaFin.Value.Date.AddDays(1), DateTimeKind.Utc));
                
                // Filtro de búsqueda por nombre de estudiante
                if (!string.IsNullOrEmpty(busqueda))
                {
                    busqueda = busqueda.Trim().ToLower();
                    query = query.Where(c => 
                        c.Estudiante != null && 
                        (c.Estudiante.Nombre.ToLower().Contains(busqueda) || 
                         c.Estudiante.Apellido.ToLower().Contains(busqueda) ||
                         (c.Estudiante.Nombre + " " + c.Estudiante.Apellido).ToLower().Contains(busqueda)));
                }

                var totalRecords = await query.CountAsync();
                var cobros = await query
                    .OrderByDescending(c => c.FechaCreacion)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var cobrosDto = cobros.Select(c => _mappingService.ToDto(c));
                var paginatedResponse = new PaginatedResponse<CobroDto>
                {
                    Data = cobrosDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<CobroDto>>.SuccessResponse(paginatedResponse, "Cobros obtenidos"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<CobroDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CobroDto>>> GetCobro(int id)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<CobroDto>.ErrorResponse("Solo administradores pueden ver cobros"));

            try
            {
                var cobro = await _context.Cobros
                    .Include(c => c.Estudiante)
                    .FirstOrDefaultAsync(c => c.IdCobro == id);

                if (cobro == null)
                    return NotFound(ApiResponse<CobroDto>.ErrorResponse("Cobro no encontrado"));

                return Ok(ApiResponse<CobroDto>.SuccessResponse(_mappingService.ToDto(cobro), "Cobro obtenido"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CobroDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpGet("estudiante/{estudianteId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CobroDto>>>> GetCobrosByEstudiante(int estudianteId)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<IEnumerable<CobroDto>>.ErrorResponse("Solo administradores pueden ver cobros"));

            try
            {
                var cobros = await _context.Cobros
                    .Include(c => c.Estudiante)
                    .Where(c => c.IdEstudiante == estudianteId)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                var cobrosDto = cobros.Select(c => _mappingService.ToDto(c));
                return Ok(ApiResponse<IEnumerable<CobroDto>>.SuccessResponse(cobrosDto, "Cobros obtenidos"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CobroDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CobroDto>>> PostCobro(CobroCreateDto cobroDto)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<CobroDto>.ErrorResponse("Solo administradores pueden crear cobros"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CobroDto>.ErrorResponse("Datos inválidos", ModelState));

            // Validar campos requeridos
            if (!cobroDto.FechaPago.HasValue)
                return BadRequest(ApiResponse<CobroDto>.ErrorResponse("La fecha de pago es requerida"));
            
            if (string.IsNullOrWhiteSpace(cobroDto.MetodoPago))
                return BadRequest(ApiResponse<CobroDto>.ErrorResponse("El método de pago es requerido"));

            try
            {
                // Verificar que el estudiante existe
                var estudiante = await _context.Personas.FindAsync(cobroDto.IdEstudiante);
                if (estudiante == null || estudiante.Rol != "estudiante")
                    return BadRequest(ApiResponse<CobroDto>.ErrorResponse("Estudiante no encontrado"));

                var cobro = _mappingService.ToEntity(cobroDto);
                _context.Cobros.Add(cobro);
                await _context.SaveChangesAsync();

                await _context.Entry(cobro).Reference(c => c.Estudiante).LoadAsync();

                // Notificar nuevo cobro via WebSocket
                var cobroResponseDto = _mappingService.ToDto(cobro);
                await _notificacionService.NotificarNuevoCobro(cobro.IdEstudiante, cobroResponseDto);

                return CreatedAtAction(nameof(GetCobro), new { id = cobro.IdCobro },
                    ApiResponse<CobroDto>.SuccessResponse(_mappingService.ToDto(cobro), "Cobro creado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CobroDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CobroDto>>> PutCobro(int id, CobroUpdateDto cobroDto)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<CobroDto>.ErrorResponse("Solo administradores pueden modificar cobros"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CobroDto>.ErrorResponse("Datos inválidos", ModelState));

            try
            {
                var cobro = await _context.Cobros
                    .Include(c => c.Estudiante)
                    .FirstOrDefaultAsync(c => c.IdCobro == id);

                if (cobro == null)
                    return NotFound(ApiResponse<CobroDto>.ErrorResponse("Cobro no encontrado"));

                _mappingService.UpdateEntity(cobro, cobroDto);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<CobroDto>.SuccessResponse(_mappingService.ToDto(cobro), "Cobro actualizado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CobroDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteCobro(int id)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Solo administradores pueden eliminar cobros"));

            try
            {
                var cobro = await _context.Cobros.FindAsync(id);
                if (cobro == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Cobro no encontrado"));

                _context.Cobros.Remove(cobro);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Cobro eliminado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Obtener estado de pago de un estudiante
        [HttpGet("estado-pago/{estudianteId}")]
        public async Task<ActionResult<ApiResponse<EstadoPagoEstudianteDto>>> GetEstadoPago(int estudianteId)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<EstadoPagoEstudianteDto>.ErrorResponse("Solo administradores pueden ver estado de pagos"));

            try
            {
                var estudiante = await _context.Personas.FindAsync(estudianteId);
                if (estudiante == null || estudiante.Rol != "estudiante")
                    return NotFound(ApiResponse<EstadoPagoEstudianteDto>.ErrorResponse("Estudiante no encontrado"));

                var anioActual = DateTime.Now.Year;
                var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
                                    "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

                // Obtener todos los cobros del estudiante
                var cobros = await _context.Cobros
                    .Where(c => c.IdEstudiante == estudianteId)
                    .OrderByDescending(c => c.FechaPago)
                    .ToListAsync();

                // Pagos mensuales del año actual - buscar por mes y año separados O por formato combinado
                var pagosMensuales = meses.Select(mes => {
                    var cobro = cobros.FirstOrDefault(c => 
                        c.TipoCobro == "mensual" && 
                        c.EstadoCobro == "pagado" &&
                        (
                            // Formato nuevo: mes separado + año separado
                            (c.MesCorrespondiente == mes && c.AnioCorrespondiente == anioActual) ||
                            // Formato antiguo: "Mes Año" combinado
                            c.MesCorrespondiente == $"{mes} {anioActual}"
                        ));
                    return new PagoMensualDto
                    {
                        Mes = mes,
                        Anio = anioActual,
                        Pagado = cobro != null,
                        FechaPago = cobro?.FechaPago,
                        Monto = cobro?.Monto
                    };
                }).ToList();

                var estadoPago = new EstadoPagoEstudianteDto
                {
                    IdEstudiante = estudianteId,
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                    PagosMensuales = pagosMensuales
                };

                return Ok(ApiResponse<EstadoPagoEstudianteDto>.SuccessResponse(estadoPago, "Estado de pago obtenido"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstadoPagoEstudianteDto>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Obtener resumen de pagos de todos los estudiantes
        [HttpGet("resumen-pagos")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ResumenPagoEstudianteDto>>>> GetResumenPagos(
            [FromQuery] int? mes = null,
            [FromQuery] int? anio = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<IEnumerable<ResumenPagoEstudianteDto>>.ErrorResponse("Solo administradores pueden ver resumen de pagos"));

            try
            {
                var anioConsulta = anio ?? DateTime.Now.Year;
                var mesConsulta = mes ?? DateTime.Now.Month;
                var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
                                    "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
                var mesNombre = meses[mesConsulta - 1];

                var estudiantes = await _context.Personas
                    .Where(p => p.Rol == "estudiante" && p.Activo)
                    .ToListAsync();

                var cobros = await _context.Cobros
                    .Where(c => c.EstadoCobro == "pagado")
                    .ToListAsync();

                var resumen = estudiantes.Select(est => {
                    // Buscar por mes y año separados O por formato combinado
                    var pagoMensual = cobros.Any(c => 
                        c.IdEstudiante == est.IdPersona && 
                        c.TipoCobro == "mensual" && 
                        (
                            // Formato nuevo: mes separado + año separado
                            (c.MesCorrespondiente == mesNombre && c.AnioCorrespondiente == anioConsulta) ||
                            // Formato antiguo: "Mes Año" combinado
                            c.MesCorrespondiente == $"{mesNombre} {anioConsulta}"
                        ));

                    return new ResumenPagoEstudianteDto
                    {
                        IdEstudiante = est.IdPersona,
                        NombreCompleto = $"{est.Nombre} {est.Apellido}",
                        PagoMes = pagoMensual,
                        TipoPago = pagoMensual ? "mensual" : "ninguno"
                    };
                }).ToList();

                return Ok(ApiResponse<IEnumerable<ResumenPagoEstudianteDto>>.SuccessResponse(resumen, "Resumen de pagos obtenido"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ResumenPagoEstudianteDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Historial de pagos de un estudiante con paginación
        [HttpGet("historial/{estudianteId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CobroDto>>>> GetHistorialPagos(
            int estudianteId,
            [FromQuery] PaginationParams pagination)
        {
            if (!EsAdministrador())
                return StatusCode(403, ApiResponse<PaginatedResponse<CobroDto>>.ErrorResponse("Solo administradores pueden ver historial de pagos"));

            try
            {
                var estudiante = await _context.Personas.FindAsync(estudianteId);
                if (estudiante == null || estudiante.Rol != "estudiante")
                    return NotFound(ApiResponse<PaginatedResponse<CobroDto>>.ErrorResponse("Estudiante no encontrado"));

                var query = _context.Cobros
                    .Include(c => c.Estudiante)
                    .Where(c => c.IdEstudiante == estudianteId)
                    .OrderByDescending(c => c.FechaPago ?? DateTime.MinValue);

                var totalRecords = await query.CountAsync();
                var cobros = await query
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var cobrosDto = cobros.Select(c => _mappingService.ToDto(c));
                var paginatedResponse = new PaginatedResponse<CobroDto>
                {
                    Data = cobrosDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<CobroDto>>.SuccessResponse(paginatedResponse, "Historial de pagos obtenido"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<CobroDto>>.ErrorResponse("Error interno", ex.Message));
            }
        }

        // Reporte CSV de cobros
        [HttpGet("reporte/csv")]
        public async Task<IActionResult> GetReporteCsv(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] string? tipoCobro = null,
            [FromQuery] string? estadoCobro = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var cobros = await ObtenerCobrosParaReporte(fechaInicio, fechaFin, idEstudiante, tipoCobro, estadoCobro);

                var csv = new System.Text.StringBuilder();
                // BOM para Excel
                csv.Append('\uFEFF');
                csv.AppendLine("ID;Fecha Pago;Estudiante;Tipo Cobro;Mes/Año;Método Pago;Estado;Monto");

                foreach (var c in cobros)
                {
                    var estudiante = $"{c.Estudiante?.Nombre} {c.Estudiante?.Apellido}".Replace(";", " ");
                    var periodo = c.MesCorrespondiente?.Replace(";", " ") ?? "";
                    var metodo = c.MetodoPago?.Replace(";", " ") ?? "";
                    
                    csv.AppendLine($"{c.IdCobro};{c.FechaPago:yyyy-MM-dd};{estudiante};{c.TipoCobro};{periodo};{metodo};{c.EstadoCobro};{c.Monto:F2}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv; charset=utf-8", $"reporte_cobros_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        // Reporte PDF de cobros
        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GetReportePdf(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idEstudiante = null,
            [FromQuery] string? tipoCobro = null,
            [FromQuery] string? estadoCobro = null)
        {
            if (!EsAdministrador())
                return StatusCode(403, "Solo administradores pueden descargar reportes");

            try
            {
                var cobros = await ObtenerCobrosParaReporte(fechaInicio, fechaFin, idEstudiante, tipoCobro, estadoCobro);
                var totalMonto = cobros.Sum(c => c.Monto);

                var html = GenerarHtmlReporteCobros(cobros, fechaInicio, fechaFin, totalMonto);
                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html; charset=utf-8", $"reporte_cobros_{DateTime.Now:yyyyMMdd}.html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar reporte: {ex.Message}");
            }
        }

        private async Task<List<Cobro>> ObtenerCobrosParaReporte(
            DateTime? fechaInicio, DateTime? fechaFin, int? idEstudiante, string? tipoCobro, string? estadoCobro)
        {
            var query = _context.Cobros.Include(c => c.Estudiante).AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaPago >= DateTime.SpecifyKind(fechaInicio.Value.Date, DateTimeKind.Utc));
            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaPago < DateTime.SpecifyKind(fechaFin.Value.Date.AddDays(1), DateTimeKind.Utc));
            if (idEstudiante.HasValue)
                query = query.Where(c => c.IdEstudiante == idEstudiante.Value);
            if (!string.IsNullOrEmpty(tipoCobro))
                query = query.Where(c => c.TipoCobro == tipoCobro);
            if (!string.IsNullOrEmpty(estadoCobro))
                query = query.Where(c => c.EstadoCobro == estadoCobro);

            return await query.OrderByDescending(c => c.FechaPago).ToListAsync();
        }

        private string GenerarHtmlReporteCobros(List<Cobro> cobros, DateTime? fechaInicio, DateTime? fechaFin, decimal totalMonto)
        {
            var periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            else if (fechaInicio.HasValue)
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            else if (fechaFin.HasValue)
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";

            var rows = string.Join("", cobros.Select(c => $@"
                <tr>
                    <td>{c.IdCobro}</td>
                    <td>{c.FechaPago:dd/MM/yyyy}</td>
                    <td>{c.Estudiante?.Nombre} {c.Estudiante?.Apellido}</td>
                    <td>{c.TipoCobro}</td>
                    <td>{c.MesCorrespondiente}</td>
                    <td>{c.MetodoPago}</td>
                    <td>{c.EstadoCobro}</td>
                    <td style='text-align:right'>${c.Monto:F2}</td>
                </tr>"));

            return $@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'><title>Reporte de Cobros</title>
<style>
    body {{ font-family: Arial, sans-serif; margin: 20px; }}
    h1 {{ color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }}
    .info {{ margin-bottom: 20px; color: #666; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
    th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
    th {{ background-color: #007bff; color: white; }}
    tr:nth-child(even) {{ background-color: #f9f9f9; }}
    .total {{ font-weight: bold; font-size: 1.2em; margin-top: 20px; text-align: right; }}
    @media print {{ body {{ margin: 0; }} }}
</style></head><body>
<h1>Reporte de Cobros - Academia de Danza</h1>
<div class='info'>
    <p>Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    <p>{periodo}</p>
    <p>Total de registros: {cobros.Count}</p>
</div>
<table>
    <thead><tr>
        <th>ID</th><th>Fecha</th><th>Estudiante</th><th>Tipo</th><th>Período</th><th>Método</th><th>Estado</th><th>Monto</th>
    </tr></thead>
    <tbody>{rows}</tbody>
</table>
<div class='total'>Total: ${totalMonto:F2}</div>
<script>window.onload = function() {{ window.print(); }}</script>
</body></html>";
        }
    }
}
