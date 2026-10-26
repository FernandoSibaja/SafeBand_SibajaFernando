namespace SafeBand.api.Contracts;


public record AlumnoEnEscuela(
    int Id,
    string Nombre,
    int? GrupoId,
    string? Grupo,
    int? SalonId,                    
    string? Pulsera,
    UltimaDeteccion? Ultima);      
