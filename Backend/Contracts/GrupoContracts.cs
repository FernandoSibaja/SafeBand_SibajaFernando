using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;


public record GuardarGrupoRequest(
    [property: Required(ErrorMessage = "El campo 'nombre' es obligatorio.")]
    [property: StringLength(50, ErrorMessage = "El campo 'nombre' admite máximo 50 caracteres.")]
    string? Nombre,

    int? ZonaId = null,

    string? MaestroId = null,

    List<int>? AlumnoIds = null);


public record AlumnoDeGrupo(int Id, string Nombre);


public record GrupoResponse(
    int Id,
    string Nombre,
    bool Activo,
    int? ZonaId,
    string? Zona,              
    string? MaestroId,
    string? Maestro,           
    List<AlumnoDeGrupo> Alumnos);  


public record MaestroResumen(string Id, string Nombre, string? Email);

public record GrupoResumen(int Id, string Nombre);
