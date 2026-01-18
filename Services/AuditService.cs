using Microsoft.EntityFrameworkCore;
using academia.Data;
using academia.Models;
using academia.DTOs;
using System.Text.Json;

namespace academia.Services
{
    public class AuditService : IAuditService
    {
        private readonly AcademiaContext _context;
        private readonly ILogger<AuditService> _logger;
        private HttpContext? _httpContext;

        public AuditService(AcademiaContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void SetHttpContext(HttpContext? context)
        {
            _httpContext = context;
        }

        public async Task RegistrarAuditAsync(AuditLog auditLog)
        {
            try
            {
                // Enriquecer con información del contexto HTTP si está disponible
                if (_httpContext != null)
                {
                    auditLog.IpAddress ??= ObtenerIpAddress();
                    auditLog.UserAgent ??= _httpContext.Request.Headers["User-Agent"].ToString();
                    auditLog.Endpoint ??= _httpContext.Request.Path.ToString();
                    auditLog.MetodoHttp ??= _httpContext.Request.Method;
                    
                    // Obtener información del usuario del token
                    if (auditLog.IdUsuario == null)
                    {
                        var userIdClaim = _httpContext.User.FindFirst("id")?.Value;
                        if (int.TryParse(userIdClaim, out int userId))
                        {
                            auditLog.IdUsuario = userId;
                        }
                        auditLog.NombreUsuario ??= _httpContext.User.FindFirst("nombre")?.Value;
                        auditLog.RolUsuario ??= _httpContext.User.FindFirst("rol")?.Value;
                    }
                }

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar auditoría para {Tabla} - {Operacion}", 
                    auditLog.TablaAfectada, auditLog.TipoOperacion);
                throw;
            }
        }

        public async Task<(List<AuditLogDto> Logs, int Total)> ObtenerLogsAsync(AuditLogFilterDto filtro)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filtro.TablaAfectada))
                query = query.Where(a => a.TablaAfectada == filtro.TablaAfectada);

            if (!string.IsNullOrEmpty(filtro.TipoOperacion))
                query = query.Where(a => a.TipoOperacion == filtro.TipoOperacion);

            if (filtro.IdUsuario.HasValue)
                query = query.Where(a => a.IdUsuario == filtro.IdUsuario);

            if (filtro.FechaDesde.HasValue)
                query = query.Where(a => a.FechaOperacion >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(a => a.FechaOperacion <= filtro.FechaHasta.Value);

            if (!string.IsNullOrEmpty(filtro.IdRegistro))
                query = query.Where(a => a.IdRegistro == filtro.IdRegistro);

            if (filtro.Exitoso.HasValue)
                query = query.Where(a => a.Exitoso == filtro.Exitoso.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.FechaOperacion)
                .Skip((filtro.Pagina - 1) * filtro.TamañoPagina)
                .Take(filtro.TamañoPagina)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return (logs, total);
        }

        public async Task<HistorialRegistroDto> ObtenerHistorialRegistroAsync(string tabla, string idRegistro)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.TablaAfectada == tabla && a.IdRegistro == idRegistro)
                .OrderByDescending(a => a.FechaOperacion)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return new HistorialRegistroDto
            {
                TablaAfectada = tabla,
                IdRegistro = idRegistro,
                Cambios = logs
            };
        }

        public async Task<AuditLogResumenDto> ObtenerResumenAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaOperacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaOperacion <= fechaHasta.Value);

            var resumen = new AuditLogResumenDto
            {
                TotalOperaciones = await query.CountAsync(),
                TotalInserts = await query.CountAsync(a => a.TipoOperacion == TipoOperacionAudit.Insert),
                TotalUpdates = await query.CountAsync(a => a.TipoOperacion == TipoOperacionAudit.Update),
                TotalDeletes = await query.CountAsync(a => a.TipoOperacion == TipoOperacionAudit.Delete),
                OperacionesFallidas = await query.CountAsync(a => !a.Exitoso)
            };

            // Operaciones por tabla
            resumen.OperacionesPorTabla = await query
                .GroupBy(a => a.TablaAfectada)
                .Select(g => new { Tabla = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Tabla, x => x.Count);

            // Operaciones por usuario
            resumen.OperacionesPorUsuario = await query
                .Where(a => a.NombreUsuario != null)
                .GroupBy(a => a.NombreUsuario!)
                .Select(g => new { Usuario = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Usuario, x => x.Count);

            // Últimas operaciones
            resumen.UltimasOperaciones = await query
                .OrderByDescending(a => a.FechaOperacion)
                .Take(10)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return resumen;
        }

        public async Task<List<AuditLogDto>> ObtenerOperacionesPorUsuarioAsync(int idUsuario, int limite = 100)
        {
            return await _context.AuditLogs
                .Where(a => a.IdUsuario == idUsuario)
                .OrderByDescending(a => a.FechaOperacion)
                .Take(limite)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        private string? ObtenerIpAddress()
        {
            if (_httpContext == null) return null;

            // Intentar obtener IP real detrás de proxy
            var forwardedFor = _httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            return _httpContext.Connection.RemoteIpAddress?.ToString();
        }

        private static AuditLogDto MapToDto(AuditLog audit)
        {
            return new AuditLogDto
            {
                IdAudit = audit.IdAudit,
                TablaAfectada = audit.TablaAfectada,
                TipoOperacion = audit.TipoOperacion,
                IdRegistro = audit.IdRegistro,
                DatosAnteriores = DeserializeJson(audit.DatosAnteriores),
                DatosNuevos = DeserializeJson(audit.DatosNuevos),
                CamposModificados = DeserializeJsonList(audit.CamposModificados),
                IdUsuario = audit.IdUsuario,
                NombreUsuario = audit.NombreUsuario,
                RolUsuario = audit.RolUsuario,
                IpAddress = audit.IpAddress,
                Endpoint = audit.Endpoint,
                MetodoHttp = audit.MetodoHttp,
                FechaOperacion = audit.FechaOperacion,
                DuracionMs = audit.DuracionMs,
                Exitoso = audit.Exitoso,
                MensajeError = audit.MensajeError
            };
        }

        private static object? DeserializeJson(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return json;
            }
        }

        private static List<string>? DeserializeJsonList(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
