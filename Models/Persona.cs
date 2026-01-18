using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("persona")]
    public class Persona
    {
        [Key]
        [Column("id_persona")]
        public int IdPersona { get; set; }
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = null!;
        
        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        [Column("apellido")]
        public string Apellido { get; set; } = null!;
        
        [Phone]
        [Column("telefono")]
        public string? Telefono { get; set; }
        
        [EmailAddress]
        [StringLength(255)]
        [Column("correo")]
        public string? Correo { get; set; }
        
        [Required]
        [StringLength(50)]
        [Column("rol")]
        public string Rol { get; set; } = "estudiante"; // administrador, profesor, estudiante, representante
        
        [Column("contrasena")]
        public string? Contrasena { get; set; }
        
        [Column("fecha_nacimiento")]
        public DateTime? FechaNacimiento { get; set; }
        
        [StringLength(10)]
        [Column("genero")]
        public string? Genero { get; set; }
        
        [Column("direccion")]
        public string? Direccion { get; set; }
        
        [StringLength(20)]
        [Column("cedula")]
        public string? Cedula { get; set; }
        
        [Column("condiciones_medicas")]
        public string? CondicionesMedicas { get; set; }
        
        // Campos específicos para profesores
        [Column("especialidad")]
        public string? Especialidad { get; set; }
        
        [Column("fecha_contrato")]
        public DateTime? FechaContrato { get; set; }
        
        [Column("salario_base")]
        public decimal? SalarioBase { get; set; }
        
        // Campos específicos para representantes
        [Column("parentesco")]
        public string? Parentesco { get; set; }
        
        [Column("id_estudiante_representado")]
        public int? IdEstudianteRepresentado { get; set; }
        
        [Column("activo")]
        public bool Activo { get; set; } = true;
        
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";
        
        [NotMapped]
        public int? Edad => FechaNacimiento?.Year != null 
            ? DateTime.UtcNow.Year - FechaNacimiento.Value.Year 
            : null;
        
        [NotMapped]
        public bool EsAdministrador => Rol == "administrador";
        
        [NotMapped]
        public bool EsProfesor => Rol == "profesor";
        
        [NotMapped]
        public bool EsEstudiante => Rol == "estudiante";
        
        [NotMapped]
        public bool EsRepresentante => Rol == "representante";

        // Navegación
        [ForeignKey("IdEstudianteRepresentado")]
        public Persona? EstudianteRepresentado { get; set; }
        
        // Clases que imparte (si es profesor)
        public ICollection<Clase> ClasesImpartidas { get; set; } = new List<Clase>();
        
        // Inscripciones (si es estudiante)
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        
        // Cobros (si es estudiante)
        public ICollection<Cobro> Cobros { get; set; } = new List<Cobro>();
        
        // Asistencias (si es estudiante)
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
        
        // Representantes del estudiante
        public ICollection<Persona> Representantes { get; set; } = new List<Persona>();
        
        // Estados creados
        public ICollection<Estado> Estados { get; set; } = new List<Estado>();
    }
}
