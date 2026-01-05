using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("estado")]
    public class Estado
    {
        [Key]
        [Column("id_estado")]
        public int IdEstado { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; }

        [Column("id_persona")]
        public int? IdPersona { get; set; }

        [Column("no_asisto")]
        public bool NoAsisto { get; set; }

        [Column("retirado")]
        public bool Retirado { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        // Navegación
        [ForeignKey("IdPersona")]
        public virtual Persona? Persona { get; set; }
    }
}
