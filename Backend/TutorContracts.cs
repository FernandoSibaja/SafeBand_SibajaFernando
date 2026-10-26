using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;

public record GuardarTutorRequest(
    [property: Required(ErrorMessage = "El campo 'nombres' es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El campo 'nombres' admite máximo 100 caracteres.")]
    string? Nombres,

    [property: Required(ErrorMessage = "El campo 'apellidoPaterno' es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El campo 'apellidoPaterno' admite máximo 100 caracteres.")]
    string? ApellidoPaterno,

    [property: Required(ErrorMessage = "El campo 'email' es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El campo 'email' no tiene un formato válido.")]
    [property: StringLength(200, ErrorMessage = "El campo 'email' admite máximo 200 caracteres.")]
    string? Email,

    [property: StringLength(100, ErrorMessage = "El campo 'apellidoMaterno' admite máximo 100 caracteres.")]
    string? ApellidoMaterno = null,

    [property: RegularExpression(@"^[0-9+\s\-()]{7,20}$",
        ErrorMessage = "El campo 'telefono' solo admite números, espacios, +, - y paréntesis (7 a 20 caracteres).")]
    string? Telefono = null,

    List<HijoRequest>? Hijos = null);


public record HijoRequest(
    int AlumnoId,
    [property: StringLength(50, ErrorMessage = "El parentesco admite máximo 50 caracteres.")]
    string? Parentesco = null,
    bool EsContactoPrincipal = false,
    bool PuedeRecoger = true);


public record VincularAlumnoRequest(
    [property: StringLength(50, ErrorMessage = "El campo 'parentesco' admite máximo 50 caracteres.")]
    string? Parentesco = null,
    bool EsContactoPrincipal = false,
    bool PuedeRecoger = true);


public record HijoResponse(
    int AlumnoId,
    string Nombre,
    string? Parentesco,
    bool EsContactoPrincipal,
    bool PuedeRecoger);


public record TutorResponse(
    int Id,
    string Nombres,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string Email,
    string? Telefono,
    bool Activo,
    DateTime FechaAlta,
    List<HijoResponse> Hijos,
    string EstadoCuenta);   


public static class EstadosCuenta
{
    public const string SinInvitar = "SinInvitar";
    public const string InvitacionPendiente = "InvitacionPendiente";
    public const string CuentaActiva = "CuentaActiva";
}


public record InvitacionResponse(string Codigo, DateTime Expira, string Email, string Tutor);
