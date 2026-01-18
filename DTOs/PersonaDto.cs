namespace academia.DTOs
{
    public class PersonaDto
    {
        public int IdPersona { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string Rol { get; set; } = null!; // administrador, profesor, estudiante, representante
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Cedula { get; set; }
        public string? CondicionesMedicas { get; set; }
        
        // Campos de profesor
        public string? Especialidad { get; set; }
        public DateTime? FechaContrato { get; set; }
        public decimal? SalarioBase { get; set; }
        
        // Campos de representante
        public string? Parentesco { get; set; }
        public int? IdEstudianteRepresentado { get; set; }
        public string? NombreEstudianteRepresentado { get; set; }
        
        public bool Activo { get; set; }
        public string NombreCompleto { get; set; } = null!;
    }

    public class PersonaCreateDto
    {
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string Rol { get; set; } = "estudiante";
        public string? Contrasena { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Cedula { get; set; }
        public string? CondicionesMedicas { get; set; }
        
        // Campos de profesor
        public string? Especialidad { get; set; }
        public DateTime? FechaContrato { get; set; }
        public decimal? SalarioBase { get; set; }
        
        // Campos de representante
        public string? Parentesco { get; set; }
        public int? IdEstudianteRepresentado { get; set; }
    }

    public class PersonaUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Rol { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Cedula { get; set; }
        public string? CondicionesMedicas { get; set; }
        
        // Campos de profesor
        public string? Especialidad { get; set; }
        public DateTime? FechaContrato { get; set; }
        public decimal? SalarioBase { get; set; }
        
        // Campos de representante
        public string? Parentesco { get; set; }
        public int? IdEstudianteRepresentado { get; set; }
        
        public bool? Activo { get; set; }
    }

    // DTO simplificado para referencias en otras entidades
    public class PersonaSimpleDto
    {
        public int IdPersona { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Rol { get; set; } = null!;
    }
}
