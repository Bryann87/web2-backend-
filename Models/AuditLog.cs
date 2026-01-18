using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace academia.Models
{
    public static class TipoOperacionAudit
    {
        public const string Insert = "INSERT";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
    }

    [Table("audit_log")]
    public class AuditLog
    {
        [Key]
        [Column("id_audit")]
        public long IdAudit { get; set; }

        [Required]
        [StringLength(100)]
        [Column("tabla_afectada")]
        public string TablaAfectada { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Column("tipo_operacion")]
        public string TipoOperacion { get; set; } = null!; // INSERT, UPDATE, DELETE

        [Column("id_registro")]
        public string? IdRegistro { get; set; }

        [Column("datos_anteriores", TypeName = "jsonb")]
        public string? DatosAnteriores { get; set; }

        [Column("datos_nuevos", TypeName = "jsonb")]
        public string? DatosNuevos { get; set; }

        [Column("campos_modificados", TypeName = "jsonb")]
        public string? CamposModificados { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }

        [StringLength(100)]
        [Column("nombre_usuario")]
        public string? NombreUsuario { get; set; }

        [StringLength(50)]
        [Column("rol_usuario")]
        public string? RolUsuario { get; set; }

        [StringLength(45)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("fecha_operacion")]
        public DateTime FechaOperacion { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        [Column("endpoint")]
        public string? Endpoint { get; set; }

        [StringLength(10)]
        [Column("metodo_http")]
        public string? MetodoHttp { get; set; }

        [Column("duracion_ms")]
        public long? DuracionMs { get; set; }

        [Column("exitoso")]
        public bool Exitoso { get; set; } = true;

        [Column("mensaje_error")]
        public string? MensajeError { get; set; }
    }
}
