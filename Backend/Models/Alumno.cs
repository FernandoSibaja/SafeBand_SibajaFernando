namespace SafeBand.api.Models;
public class Alumno
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }        
    public DateOnly? FechaNacimiento { get; set; }       
    public string? Matricula { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public int? GrupoId { get; set; }
    public Grupo? Grupo { get; set; }
    public List<Pulsera> Pulseras { get; set; } = [];
    public List<TutorAlumno> Tutores { get; set; } = [];
}
