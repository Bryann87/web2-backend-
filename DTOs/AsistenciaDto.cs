namespace academia.DTOs
{
    public class AsistenciaDto
    {
        public int IdAsist { get; set; }
        public DateTime FechaAsis { get; set; }
        public string? EstadoAsis { get; set; }
        public string? Observaciones { get; set; }
        public PersonaSimpleDto? Estudiante { get; set; }
        public ClaseSimpleDto? Clase { get; set; }
    }

    public class AsistenciaCreateDto
    {
        public DateTime FechaAsis { get; set; }
        public string EstadoAsis { get; set; } = "presente";
        public string? Observaciones { get; set; }
        public int IdEstudiante { get; set; }
        public int IdClase { get; set; }
    }

    public class AsistenciaUpdateDto
    {
        public DateTime FechaAsis { get; set; }
        public string EstadoAsis { get; set; } = "presente";
        public string? Observaciones { get; set; }
        public int IdEstudiante { get; set; }
        public int IdClase { get; set; }
    }

    public class ValidacionAsistenciaDto
    {
        public bool PuedeRegistrar { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string DiaSemanaClase { get; set; } = string.Empty;
        public string DiaActual { get; set; } = string.Empty;
        public bool YaRegistradaEstaSemana { get; set; }
        public string? FechaUltimaAsistencia { get; set; }
        public string? ProximaFechaDisponible { get; set; }
        // Campos de debug para verificar zona horaria
        public string? HoraServidorUtc { get; set; }
        public string? HoraServidorEcuador { get; set; }
        public string? ZonaHorariaUsada { get; set; }
    }
}
