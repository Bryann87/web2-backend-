namespace academia.Services
{
    public interface INotificacionService
    {
        Task NotificarATodos(string tipo, object datos);
        Task NotificarAGrupo(string grupo, string tipo, object datos);
        Task NotificarAUsuario(string usuarioId, string tipo, object datos);
        Task NotificarNuevaAsistencia(int idClase, object asistencia, string? nombreProfesor = null);
        Task NotificarNuevoEstudiante(object estudiante);
        Task NotificarNuevoCobro(int idEstudiante, object cobro);
        Task NotificarCambioClase(int idClase, object clase);
    }
}
