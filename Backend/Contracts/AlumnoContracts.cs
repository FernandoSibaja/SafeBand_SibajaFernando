using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;


public record GuardarAlumnoRequest(
    [property: Required(ErrorMessage = "El campo 'nombres' es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El campo 'nombres' admite máximo 100 caracteres.")]
    string? Nombres,

    [property: Required(ErrorMessage = "El campo 'apellidoPaterno' es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El campo 'apellidoPaterno' admite máximo 100 caracteres.")]
    string? ApellidoPaterno,

    [property: StringLength(100, ErrorMessage = "El campo 'apellidoMaterno' admite máximo 100 caracteres.")]
    string? ApellidoMaterno = null,

    DateOnly? FechaNacimiento = null,

    [property: StringLength(30, ErrorMessage = "El campo 'matricula' admite máximo 30 caracteres.")]
    string? Matricula = null,


    int? PulseraId = null,

    // Grupo escolar. null = sin grupo.
    int? GrupoId = null);


public record PulseraResumen(int Id, string IdentificadorBle);


public record AlumnoResponse(
    int Id,
    string Nombres,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    DateOnly? FechaNacimiento,
    string? Matricula,
    bool Activo,
    DateTime FechaAlta,
    PulseraResumen? Pulsera,
    GrupoResumen? Grupo);
