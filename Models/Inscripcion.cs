using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("inscripcion")]
    public class Inscripcion
    {
        [Key]
        [Column("id_insc")]
        public int IdInsc { get; set; }
        
        [Column("fecha_insc")]
        public DateTime FechaInsc { get; set; } = DateTime.UtcNow;
        
        [Column("estado")]
        public string Estado { get; set; } = "Activa";
        
        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }
        
        [Column("motivo_baja")]
        public string? MotivoBaja { get; set; }

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
    }
}
