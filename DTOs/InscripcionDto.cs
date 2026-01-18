namespace academia.DTOs
{
    public class InscripcionDto
    {
        public int IdInsc { get; set; }
        public DateTime FechaInsc { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaBaja { get; set; }
        public string? MotivoBaja { get; set; }
        public PersonaSimpleDto? Estudiante { get; set; }
        public ClaseSimpleDto? Clase { get; set; }
    }

    public class InscripcionCreateDto
    {
        public DateTime FechaInsc { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Activa";
        public int IdEstudiante { get; set; }
        public int IdClase { get; set; }
    }

    public class InscripcionUpdateDto
    {
        public DateTime? FechaInsc { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaBaja { get; set; }
        public string? MotivoBaja { get; set; }
        public int? IdEstudiante { get; set; }
        public int? IdClase { get; set; }
    }
}
