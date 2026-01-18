namespace academia.DTOs
{
    public class AuditLogDto
    {
        public long IdAudit { get; set; }
        public string TablaAfectada { get; set; } = null!;
        public string TipoOperacion { get; set; } = null!;
        public string? IdRegistro { get; set; }
        public object? DatosAnteriores { get; set; }
        public object? DatosNuevos { get; set; }
        public List<string>? CamposModificados { get; set; }
        public int? IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? RolUsuario { get; set; }
        public string? IpAddress { get; set; }
        public string? Endpoint { get; set; }
        public string? MetodoHttp { get; set; }
        public DateTime FechaOperacion { get; set; }
        public long? DuracionMs { get; set; }
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }
    
    public class AuditLogFilterDto
    {
        public string? TablaAfectada { get; set; }
        public string? TipoOperacion { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? IdRegistro { get; set; }
        public bool? Exitoso { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 50;
    }
    
    public class AuditLogResumenDto
    {
        public int TotalOperaciones { get; set; }
        public int TotalInserts { get; set; }
        public int TotalUpdates { get; set; }
        public int TotalDeletes { get; set; }
        public int OperacionesFallidas { get; set; }
        public Dictionary<string, int> OperacionesPorTabla { get; set; } = new();
        public Dictionary<string, int> OperacionesPorUsuario { get; set; } = new();
        public List<AuditLogDto> UltimasOperaciones { get; set; } = new();
    }
    
    public class CambioDetalladoDto
    {
        public string Campo { get; set; } = null!;
        public object? ValorAnterior { get; set; }
        public object? ValorNuevo { get; set; }
    }
    
    public class HistorialRegistroDto
    {
        public string TablaAfectada { get; set; } = null!;
        public string IdRegistro { get; set; } = null!;
        public List<AuditLogDto> Cambios { get; set; } = new();
    }
}
