namespace SafeBand.api.Models;


public class Invitacion
{
    public int Id { get; set; }
    public int? TutorId { get; set; }
    public Tutor? Tutor { get; set; }
    public string? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string CodigoHash { get; set; } = string.Empty;
    public DateTimz Creada { get; set; }
    public DateTime Expira { get; set; }
    public DateTime? UsadaEn { get; set; }
}
