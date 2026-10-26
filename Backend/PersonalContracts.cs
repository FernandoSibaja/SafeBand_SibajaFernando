using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;


public record GuardarPersonalRequest(
    [property: Required(ErrorMessage = "El campo 'nombre' es obligatorio.")]
    [property: StringLength(150, ErrorMessage = "El campo 'nombre' admite máximo 150 caracteres.")]
    string? Nombre,

    [property: Required(ErrorMessage = "El campo 'email' es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El campo 'email' no tiene un formato válido.")]
    [property: StringLength(200, ErrorMessage = "El campo 'email' admite máximo 200 caracteres.")]
    string? Email,

    [property: Required(ErrorMessage = "El campo 'rol' es obligatorio (Maestro o Administrador).")]
    [property: RegularExpression("^(Maestro|Administrador)$", ErrorMessage = "El campo 'rol' debe ser Maestro o Administrador.")]
    string? Rol);

public record PersonalResponse(
    string Id,
    string Nombre,
    string Email,
    string Rol,
    bool Activo,
    string EstadoCuenta,       
    List<string> Grupos,        
    bool EresTu);               
