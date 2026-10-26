namespace SafeBand.api.Models;
public class TutorAlumno
{
    public int TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;

    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;

    public string? Parentesco { get; set; }

    public bool EsContactoPrincipal { get; set; }

    public bool PuedeRecoger { get; set; } = true;
}
