namespace academia.DTOs
{
    public class CobroDto
    {
        public int IdCobro { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? MetodoPago { get; set; }
        public string? MesCorrespondiente { get; set; }
        public string? EstadoCobro { get; set; }
        public string? Observaciones { get; set; }
        public string TipoCobro { get; set; } = "mensual";
        public int? AnioCorrespondiente { get; set; }
        public PersonaSimpleDto? Estudiante { get; set; }
    }

    public class CobroCreateDto
    {
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? MetodoPago { get; set; }
        public string MesCorrespondiente { get; set; } = null!;
        public string EstadoCobro { get; set; } = "pendiente";
        public string? Observaciones { get; set; }
        public string TipoCobro { get; set; } = "mensual";
        public int? AnioCorrespondiente { get; set; }
        public int IdEstudiante { get; set; }
    }

    public class CobroUpdateDto
    {
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? MetodoPago { get; set; }
        public string? MesCorrespondiente { get; set; }
        public string? EstadoCobro { get; set; }
        public string? Observaciones { get; set; }
        public string? TipoCobro { get; set; }
        public int? AnioCorrespondiente { get; set; }
    }

    // DTO para estado de pago de un estudiante
    public class EstadoPagoEstudianteDto
    {
        public int IdEstudiante { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public List<PagoMensualDto> PagosMensuales { get; set; } = new();
    }

    public class PagoMensualDto
    {
        public string Mes { get; set; } = null!;
        public int Anio { get; set; }
        public bool Pagado { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal? Monto { get; set; }
    }

    // DTO para resumen de pagos de estudiantes
    public class ResumenPagoEstudianteDto
    {
        public int IdEstudiante { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public bool PagoMes { get; set; }
        public string TipoPago { get; set; } = "ninguno";
    }
}
