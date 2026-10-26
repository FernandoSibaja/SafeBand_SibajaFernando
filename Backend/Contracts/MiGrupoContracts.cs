namespace SafeBand.api.Contracts;


public record MiGrupoResponse(
    int Id,
    string Nombre,
    int? ZonaId,                    
    string? Zona,
    List<AlumnoDeMiGrupo> Alumnos);  


public record AlumnoDeMiGrupo(
    int Id,
    string Nombre,
    string? Pulsera,                
    UltimaDeteccion? Ultima);        
