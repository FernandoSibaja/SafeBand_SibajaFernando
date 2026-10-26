namespace SafeBand.api.Models;
public class Alerta
{
    public int Id { get; set; }
    public TipoAlerta Tipo { get; set; }

    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;
    public int? ZonaId { get; set; }
    public Zona? Zona { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public OrigenDesactivacion? DesactivadaDesde { get; set; }
    public string? DesactivadaPorId { get; set; }
    public Usuario? DesactivadaPor { get; set; }
}

public enum TipoAlerta
{
    Sos,
}

public enum OrigenDesactivacion
{
    Pulsera,
    Pagina,
}
