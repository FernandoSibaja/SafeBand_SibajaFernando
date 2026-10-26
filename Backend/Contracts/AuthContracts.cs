using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;


public record LoginRequest(
    [property: Required(ErrorMessage = "El campo 'email' es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El campo 'email' no tiene un formato válido.")]
    string? Email,

    [property: Required(ErrorMessage = "El campo 'password' es obligatorio.")]
    string? Password,

    // true = la sesión sobrevive aunque se cierre el navegador (hasta que expire)
    bool Recordarme = false);


public record RegistroRequest(
    [property: Required(ErrorMessage = "El campo 'email' es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El campo 'email' no tiene un formato válido.")]
    string? Email,

    [property: Required(ErrorMessage = "El código de invitación es obligatorio.")]
    [property: StringLength(20, ErrorMessage = "El código de invitación no es válido.")]
    string? Codigo,

    [property: Required(ErrorMessage = "La contraseña es obligatoria.")]
    [property: StringLength(100, ErrorMessage = "La contraseña admite máximo 100 caracteres.")]
    string? Password,

    bool AceptaAvisoPrivacidad = false);

public record UsuarioActualResponse(
    string Id,
    string Email,
    string Nombre,
    List<string> Roles,
    int? TutorId);
