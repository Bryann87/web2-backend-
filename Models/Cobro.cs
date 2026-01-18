using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    [Table("cobro")]
    public class Cobro
    {
        [Key]
        [Column("id_cobro")]
        public int IdCobro { get; set; }
        
        [Required]
        [Column("monto")]
        [Range(0, double.MaxValue)]
        public decimal Monto { get; set; }
        
        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }
        
        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }
        
        [Column("metodo_pago")]
        public string? MetodoPago { get; set; }
        
        [Required]
        [Column("mes_correspondiente")]
        public string MesCorrespondiente { get; set; } = null!;
        
        [Column("estado_cobro")]
        public string EstadoCobro { get; set; } = "pendiente";
        
        [Column("observaciones")]
        public string? Observaciones { get; set; }
        
        // Tipo de cobro: mensual
        [Column("tipo_cobro")]
        public string TipoCobro { get; set; } = "mensual";
        
        [Column("anio_correspondiente")]
        public int? AnioCorrespondiente { get; set; }
        
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones - Estudiante es una Persona con rol "estudiante"
        [Required]
        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        // Navegación
        [ForeignKey("IdEstudiante")]
        public Persona Estudiante { get; set; } = null!;

        // Propiedades calculadas
        [NotMapped]
        public bool EstaPagado => EstadoCobro == "pagado";
        
        [NotMapped]
        public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento < DateTime.UtcNow && !EstaPagado;
        
        [NotMapped]
        public int DiasVencido => EstaVencido ? (DateTime.UtcNow - FechaVencimiento!.Value).Days : 0;
    }
}
