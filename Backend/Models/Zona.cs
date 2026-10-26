namespace SafeBand.api.Models;

public class Zona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoZona Tipo { get; set; }
    public bool Activa { get; set; } = true;

    public double? PlanoX { get; set; }
    public double? PlanoY { get; set; }
    public double? PlanoAncho { get; set; }
    public double? PlanoAlto { get; set; }
    public ModoUbicacion? Ubicacion { get; set; }

    public List<Nodo> Nodos { get; set; } = [];
    public static ModoUbicacion PorDefecto(TipoZona tipo) =>
        tipo is TipoZona.Salon or TipoZona.Tienda ? ModoUbicacion.PorNodo : ModoUbicacion.Triangulacion;
}


public enum ModoUbicacion
{
    PorNodo,
    Triangulacion
}

public enum TipoZona
{
    Salon,
    Patio,
    Tienda,
    Entrada,
    Perimetro,
    Otra
}
