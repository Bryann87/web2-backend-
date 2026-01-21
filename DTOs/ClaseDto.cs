namespace academia.DTOs
{
    public class ClaseDto
    {
        public int IdClase { get; set; }
        public string? NombreClase { get; set; }
        public string? DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
        public int DuracionMinutos { get; set; }
        public int CapacidadMax { get; set; }
        public decimal PrecioMensuClas { get; set; }
        public bool Activa { get; set; }
        public PersonaSimpleDto? Profesor { get; set; }
        public EstiloDanzaDto? EstiloDanza { get; set; }
        
        // Propiedades calculadas
        public int EstudiantesInscritos { get; set; }
        public int CuposDisponibles { get; set; }
        public bool TieneCuposDisponibles { get; set; }
    }

    public class ClaseSimpleDto
    {
        public int IdClase { get; set; }
        public string? NombreClase { get; set; }
        public string? DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
    }

    public class ClaseCreateDto
    {
        public string NombreClase { get; set; } = null!;
        public string DiaSemana { get; set; } = null!;
        public TimeSpan Hora { get; set; }
        public int DuracionMinutos { get; set; }
        public int CapacidadMax { get; set; }
        public decimal PrecioMensuClas { get; set; }
        public int IdProfesor { get; set; }
        public int IdEstilo { get; set; }
    }

    public class ClaseUpdateDto
    {
        public string NombreClase { get; set; } = null!;
        public string DiaSemana { get; set; } = null!;
        public TimeSpan Hora { get; set; }
        public int DuracionMinutos { get; set; }
        public int CapacidadMax { get; set; }
        public decimal PrecioMensuClas { get; set; }
        public int IdProfesor { get; set; }
        public int IdEstilo { get; set; }
        public bool Activa { get; set; }
    }
}
