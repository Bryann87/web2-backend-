using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("estilo_danza")]
    public class EstiloDanza
    {
        [Key]
        [Column("id_estilo")]
        public int IdEstilo { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre_esti")]
        public string NombreEsti { get; set; } = null!;
        
        [Column("descripcion")]
        public string? Descripcion { get; set; }
        
        [Column("nivel_dificultad")]
        public string NivelDificultad { get; set; } = "Principiante";
        
        [Column("edad_minima")]
        public int? EdadMinima { get; set; }
        
        [Column("edad_maxima")]
        public int? EdadMaxima { get; set; }
        
        [Column("activo")]
        public bool Activo { get; set; } = true;
        
        [Column("precio_base")]
        public decimal? PrecioBase { get; set; }

        // Navegación
        public ICollection<Clase> Clases { get; set; } = new List<Clase>();

        // Propiedades calculadas
        [NotMapped]
        public int ClasesActivas => Clases?.Count(c => c.Activa) ?? 0;
        
        [NotMapped]
        public string RangoEdad => EdadMinima.HasValue && EdadMaxima.HasValue 
            ? $"{EdadMinima}-{EdadMaxima} años"
            : EdadMinima.HasValue 
                ? $"Desde {EdadMinima} años"
                : EdadMaxima.HasValue 
                    ? $"Hasta {EdadMaxima} años"
                    : "Todas las edades";
    }
}
