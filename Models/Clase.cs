using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("clase")]
    public class Clase
    {
        [Key]
        [Column("id_clase")]
        public int IdClase { get; set; }
        
        [Required]
        [StringLength(100)]
        [Column("nombre_clase")]
        public string NombreClase { get; set; } = null!;
        
        [Required]
        [Column("dia_semana")]
        public string DiaSemana { get; set; } = null!;
        
        [Required]
        [Column("hora")]
        public TimeSpan Hora { get; set; }
        
        [Required]
        [Column("duracion_minutos")]
        public int DuracionMinutos { get; set; }
        
        [Column("capacidad_max")]
        public int CapacidadMax { get; set; } = 20;
        
        [Required]
        [Column("precio_mensu_clas")]
        [Range(0, double.MaxValue)]
        public decimal PrecioMensuClas { get; set; }
        
        [Column("activa")]
        public bool Activa { get; set; } = true;
        
        [Column("fecha_inicio")]
        public DateTime? FechaInicio { get; set; }
        
        [Column("fecha_fin")]
        public DateTime? FechaFin { get; set; }

        // Relaciones - Profesor es una Persona con rol "profesor"
        [Required]
        [Column("id_profesor")]
        public int IdProfesor { get; set; }
        
        [Required]
        [Column("id_estilo")]
        public int IdEstilo { get; set; }

        // Navegación
        [ForeignKey("IdProfesor")]
        public Persona Profesor { get; set; } = null!;
        
        [ForeignKey("IdEstilo")]
        public EstiloDanza EstiloDanza { get; set; } = null!;
        
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();

        // Propiedades calculadas
        [NotMapped]
        public TimeSpan HoraFin => Hora.Add(TimeSpan.FromMinutes(DuracionMinutos));
        
        [NotMapped]
        public int EstudiantesInscritos => Inscripciones?.Count(i => i.Estado.ToLower() == "activa") ?? 0;
        
        [NotMapped]
        public int CuposDisponibles => CapacidadMax - EstudiantesInscritos;
        
        [NotMapped]
        public bool TieneCuposDisponibles => CuposDisponibles > 0;
    }
}
