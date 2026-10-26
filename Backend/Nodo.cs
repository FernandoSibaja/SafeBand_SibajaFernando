namespace SafeBand.api.Models;

public class Nodo
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int ZonaId { get; set; }
    public Zona Zona { get; set; } = null!;

    public int UmbralRssi { get; set; } = -90;   

    public double? PlanoX { get; set; }
    public double? PlanoY { get; set; }

    public bool EsPortatil { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimoContacto { get; set; }
}
