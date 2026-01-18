using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("asistencia")]
    public class Asistencia
    {
        [Key]
        [Column("id_asist")]
        public int IdAsist { get; set; }
        
        [Column("fecha_asis")]
        public DateTime FechaAsis { get; set; }
        
        [Column("estado_asis")]
        public string EstadoAsis { get; set; } = "presente";
        
        [Column("observaciones")]
        public string? Observaciones { get; set; }
        
        [Column("registrada_por")]
        public int? RegistradaPor { get; set; }

        // Relaciones - Estudiante es una Persona con rol "estudiante"
        [Required]
        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Required]
        [Column("id_clase")]
        public int IdClase { get; set; }

        // Navegación
        [ForeignKey("IdEstudiante")]
        public Persona Estudiante { get; set; } = null!;

        [ForeignKey("IdClase")]
        public Clase Clase { get; set; } = null!;
        
        [ForeignKey("RegistradaPor")]
        public Persona? RegistradaPorPersona { get; set; }
    }
}
