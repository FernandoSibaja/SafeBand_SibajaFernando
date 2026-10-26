using SafeBand.api.Models;

namespace SafeBand.api.Contracts;


public record AlertaResponse(
    int Id,
    TipoAlerta Tipo,
    int AlumnoId,
    string Alumno,
    string? Grupo,
    string? Zona,                         
    DateTime Inicio,
    DateTime? Fin,                         
    OrigenDesactivacion? DesactivadaDesde, 
    string? DesactivadaPor,                
    bool PuedoDesactivar);                