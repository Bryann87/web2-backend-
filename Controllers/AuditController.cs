using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using academia.Services;
using academia.DTOs;

namespace academia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los logs de auditoría con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> ObtenerLogs([FromQuery] AuditLogFilterDto filtro)
        {
            try
            {
                // Solo administradores pueden ver la auditoría
                var rolUsuario = User.FindFirst("rol")?.Value;
                if (rolUsuario != "administrador")
                {
                    return Forbid();
                }

                var (logs, total) = await _auditService.ObtenerLogsAsync(filtro);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logs de auditoría obtenidos correctamente",
                    Data = new
                    {
                        logs,
                        total,
                        pagina = filtro.Pagina,
                        tamañoPagina = filtro.TamañoPagina,
                        totalPaginas = (int)Math.Ceiling((double)total / filtro.TamañoPagina)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs de auditoría");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error interno al obtener logs de auditoría"
                });
            }
        }

        /// <summary>
        /// Obtiene el historial de cambios de un registro específico
        /// </summary>
        [HttpGet("historial/{tabla}/{idRegistro}")]
        public async Task<ActionResult<ApiResponse<HistorialRegistroDto>>> ObtenerHistorial(string tabla, string idRegistro)
        {
            try
            {
                var rolUsuario = User.FindFirst("rol")?.Value;
                if (rolUsuario != "administrador")
                {
                    return Forbid();
                }

                var historial = await _auditService.ObtenerHistorialRegistroAsync(tabla, idRegistro);

                return Ok(new ApiResponse<HistorialRegistroDto>
                {
                    Success = true,
                    Message = "Historial obtenido correctamente",
                    Data = historial
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de {Tabla}/{Id}", tabla, idRegistro);
                return StatusCode(500, new ApiResponse<HistorialRegistroDto>
                {
                    Success = false,
                    Message = "Error interno al obtener historial"
                });
            }
        }

        /// <summary>
        /// Obtiene un resumen de las operaciones de auditoría
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ApiResponse<AuditLogResumenDto>>> ObtenerResumen(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            try
            {
                var rolUsuario = User.FindFirst("rol")?.Value;
                if (rolUsuario != "administrador")
                {
                    return Forbid();
                }

                var resumen = await _auditService.ObtenerResumenAsync(fechaDesde, fechaHasta);

                return Ok(new ApiResponse<AuditLogResumenDto>
                {
                    Success = true,
                    Message = "Resumen de auditoría obtenido correctamente",
                    Data = resumen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de auditoría");
                return StatusCode(500, new ApiResponse<AuditLogResumenDto>
                {
                    Success = false,
                    Message = "Error interno al obtener resumen"
                });
            }
        }

        /// <summary>
        /// Obtiene las operaciones realizadas por un usuario específico
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> ObtenerPorUsuario(int idUsuario, [FromQuery] int limite = 100)
        {
            try
            {
                var rolUsuario = User.FindFirst("rol")?.Value;
                if (rolUsuario != "administrador")
                {
                    return Forbid();
                }

                var logs = await _auditService.ObtenerOperacionesPorUsuarioAsync(idUsuario, limite);

                return Ok(new ApiResponse<List<AuditLogDto>>
                {
                    Success = true,
                    Message = "Operaciones del usuario obtenidas correctamente",
                    Data = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener operaciones del usuario {IdUsuario}", idUsuario);
                return StatusCode(500, new ApiResponse<List<AuditLogDto>>
                {
                    Success = false,
                    Message = "Error interno al obtener operaciones del usuario"
                });
            }
        }

        /// <summary>
        /// Obtiene las tablas disponibles para filtrar
        /// </summary>
        [HttpGet("tablas")]
        public ActionResult<ApiResponse<List<string>>> ObtenerTablas()
        {
            var rolUsuario = User.FindFirst("rol")?.Value;
            if (rolUsuario != "administrador")
            {
                return Forbid();
            }

            var tablas = new List<string>
            {
                "persona",
                "clase",
                "inscripcion",
                "cobro",
                "asistencia",
                "estilo_danza",
                "estado"
            };

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Message = "Tablas disponibles",
                Data = tablas
            });
        }

        /// <summary>
        /// Obtiene los tipos de operación disponibles
        /// </summary>
        [HttpGet("tipos-operacion")]
        public ActionResult<ApiResponse<List<string>>> ObtenerTiposOperacion()
        {
            var rolUsuario = User.FindFirst("rol")?.Value;
            if (rolUsuario != "administrador")
            {
                return Forbid();
            }

            var tipos = new List<string> { "INSERT", "UPDATE", "DELETE" };

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Message = "Tipos de operación disponibles",
                Data = tipos
            });
        }
    }
}
