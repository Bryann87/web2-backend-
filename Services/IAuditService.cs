using academia.Models;
using academia.DTOs;

namespace academia.Services
{
    public interface IAuditService
    {
        /// <summary>
        /// Registra una operación de auditoría
        /// </summary>
        Task RegistrarAuditAsync(AuditLog auditLog);
        
        /// <summary>
        /// Obtiene logs de auditoría con filtros
        /// </summary>
        Task<(List<AuditLogDto> Logs, int Total)> ObtenerLogsAsync(AuditLogFilterDto filtro);
        
        /// <summary>
        /// Obtiene el historial completo de un registro específico
        /// </summary>
        Task<HistorialRegistroDto> ObtenerHistorialRegistroAsync(string tabla, string idRegistro);
        
        /// <summary>
        /// Obtiene un resumen de las operaciones de auditoría
        /// </summary>
        Task<AuditLogResumenDto> ObtenerResumenAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        
        /// <summary>
        /// Obtiene las operaciones realizadas por un usuario específico
        /// </summary>
        Task<List<AuditLogDto>> ObtenerOperacionesPorUsuarioAsync(int idUsuario, int limite = 100);
        
        /// <summary>
        /// Establece el contexto HTTP actual para capturar información del request
        /// </summary>
        void SetHttpContext(HttpContext? context);
    }
}
