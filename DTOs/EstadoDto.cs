namespace academia.DTOs
{
    public class EstadoDto
    {
        public int IdEstado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public bool NoAsisto { get; set; }
        public bool Retirado { get; set; }
        public bool Activo { get; set; }
        public PersonaSimpleDto? Persona { get; set; }
    }

    public class EstadoCreateDto
    {
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        public bool NoAsisto { get; set; }
        public bool Retirado { get; set; }
        public bool Activo { get; set; } = true;
        public int? IdPersona { get; set; }
    }

    public class EstadoUpdateDto
    {
        public DateTime FechaActualizacion { get; set; }
        public bool NoAsisto { get; set; }
        public bool Retirado { get; set; }
        public bool Activo { get; set; }
    }
}