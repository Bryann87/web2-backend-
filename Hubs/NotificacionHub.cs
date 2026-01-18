using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace academia.Hubs
{
    [Authorize]
    public class NotificacionHub : Hub
    {
        private readonly ILogger<NotificacionHub> _logger;

        public NotificacionHub(ILogger<NotificacionHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("id")?.Value;
            var tipoPersona = Context.User?.FindFirst("tipo_persona")?.Value;
            var rol = Context.User?.FindFirst("rol")?.Value;
            
            // Usar tipo_persona o rol como fallback
            var grupo = tipoPersona ?? rol;
            
            _logger.LogInformation($"Usuario conectado: {userId} - Tipo: {tipoPersona} - Rol: {rol} - Grupo asignado: {grupo}");
            
            // Log de todos los claims para depuración
            if (Context.User?.Claims != null)
            {
                foreach (var claim in Context.User.Claims)
                {
                    _logger.LogDebug($"Claim: {claim.Type} = {claim.Value}");
                }
            }
            
            // Agregar a grupo según tipo de persona o rol
            if (!string.IsNullOrEmpty(grupo))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
                _logger.LogInformation($"Usuario {userId} agregado al grupo: {grupo}");
            }
            else
            {
                _logger.LogWarning($"Usuario {userId} no tiene tipo_persona ni rol definido");
            }
            
            // Grupo general para todos los usuarios autenticados
            await Groups.AddToGroupAsync(Context.ConnectionId, "usuarios");
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("id")?.Value;
            _logger.LogInformation($"Usuario desconectado: {userId}");
            
            await base.OnDisconnectedAsync(exception);
        }

        // Unirse a un grupo específico (ej: clase específica)
        public async Task UnirseAGrupo(string grupoNombre)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grupoNombre);
            _logger.LogInformation($"Usuario {Context.ConnectionId} se unió al grupo {grupoNombre}");
        }

        // Salir de un grupo específico
        public async Task SalirDeGrupo(string grupoNombre)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupoNombre);
            _logger.LogInformation($"Usuario {Context.ConnectionId} salió del grupo {grupoNombre}");
        }
    }
}
