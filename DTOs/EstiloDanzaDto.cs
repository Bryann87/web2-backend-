namespace academia.DTOs
{
    public class EstiloDanzaDto
    {
        public int IdEstilo { get; set; }
        public string NombreEsti { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string NivelDificultad { get; set; } = "Principiante";
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public bool Activo { get; set; }
        public decimal? PrecioBase { get; set; }
    }

    public class EstiloDanzaCreateDto
    {
        public string NombreEsti { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string NivelDificultad { get; set; } = "Principiante";
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public bool Activo { get; set; } = true;
        public decimal? PrecioBase { get; set; }
    }

    public class EstiloDanzaUpdateDto
    {
        public string NombreEsti { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string NivelDificultad { get; set; } = "Principiante";
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public bool Activo { get; set; }
        public decimal? PrecioBase { get; set; }
    }
}