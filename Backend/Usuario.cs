using Microsoft.AspNetCore.Identity;

namespace SafeBand.api.Models;

public class Usuario : IdentityUser
{
    public string NombreMostrar { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }

    public int? TutorId { get; set; }
    public Tutor? Tutor { get; set; }
    public DateTime? AvisoPrivacidadAceptado { get; set; }

    public long? TelegramChatId { get; set; }
    public string? TelegramCodigo { get; set; }
    public DateTime? TelegramCodigoExpira { get; set; }
}
