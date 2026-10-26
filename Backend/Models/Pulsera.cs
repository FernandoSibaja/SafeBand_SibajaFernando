namespace SafeBand.api.Models;

public class Pulsera
{
    public int Id { get; set; }

    public string IdentificadorBle { get; set; } = string.Empty;

    public string? UidNfc { get; set; }

    public int? AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public bool Activa { get; set; } = true;
    public DateTime FechaAlta { get; set; }

    public DateTime? UltimaLectura { get; set; }
}
