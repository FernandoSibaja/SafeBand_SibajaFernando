namespace SafeBand.api.Models;

public class Tutor
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }

    public List<TutorAlumno> Alumnos { get; set; } = [];

    public List<Invitacion> Invitaciones { get; set; } = [];
}
