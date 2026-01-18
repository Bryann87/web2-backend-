using Microsoft.AspNetCore.SignalR;
using academia.Hubs;

namespace academia.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly IHubContext<NotificacionHub> _hubContext;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(IHubContext<NotificacionHub> hubContext, ILogger<NotificacionService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotificarATodos(string tipo, object datos)
        {
            try
            {
                await _hubContext.Clients.Group("usuarios").SendAsync("Notificacion", new
                {
                    tipo = tipo,
                    datos = datos,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
                _logger.LogInformation($"Notificacion enviada a todos: {tipo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificacion a todos: {tipo}");
            }
        }

        public async Task NotificarAGrupo(string grupo, string tipo, object datos)
        {
            try
            {
                await _hubContext.Clients.Group(grupo).SendAsync("Notificacion", new
                {
                    tipo = tipo,
                    datos = datos,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
                _logger.LogInformation($"Notificacion enviada al grupo {grupo}: {tipo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificacion al grupo {grupo}: {tipo}");
            }
        }

        public async Task NotificarAUsuario(string usuarioId, string tipo, object datos)
        {
            try
            {
                await _hubContext.Clients.User(usuarioId).SendAsync("Notificacion", new
                {
                    tipo = tipo,
                    datos = datos,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
                _logger.LogInformation($"Notificacion enviada al usuario {usuarioId}: {tipo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificacion al usuario {usuarioId}: {tipo}");
            }
        }

        public async Task NotificarNuevaAsistencia(int idClase, object asistencia, string? nombreProfesor = null)
        {
            // Notificar solo a administradores con el nombre del profesor
            var datosNotificacion = new
            {
                asistencia = asistencia,
                nombreProfesor = nombreProfesor ?? "Desconocido"
            };
            await NotificarAGrupo("administrador", "nueva_asistencia", datosNotificacion);
        }

        public async Task NotificarNuevoEstudiante(object estudiante)
        {
            await NotificarAGrupo("administrador", "nuevo_estudiante", estudiante);
        }

        public async Task NotificarNuevoCobro(int idEstudiante, object cobro)
        {
            // Notificar a todos los usuarios conectados para actualizar tablas en tiempo real
            await NotificarATodos("nuevo_cobro", cobro);
        }

        public async Task NotificarCambioClase(int idClase, object clase)
        {
            await NotificarAGrupo($"clase_{idClase}", "cambio_clase", clase);
            await NotificarAGrupo("administrador", "cambio_clase", clase);
        }
    }
}
