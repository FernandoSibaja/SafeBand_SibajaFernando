namespace SafeBand.api.Models;


public class Grupo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int? ZonaId { get; set; }
    public Zona? Zona { get; set; }
    public string? MaestroId { get; set; }
    public Usuario? Maestro { get; set; }
    public List<Alumno> Alumnos { get; set; } = [];
}
