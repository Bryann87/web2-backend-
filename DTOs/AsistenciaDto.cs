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
}
