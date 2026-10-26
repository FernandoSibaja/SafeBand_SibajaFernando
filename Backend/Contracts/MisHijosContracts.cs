namespace SafeBand.api.Contracts;


public record MiHijoResponse(
    int AlumnoId,
    string Nombre,
    string? Parentesco,
    string? Pulsera,                 
    UltimaDeteccion? Ultima,
    List<TramoRecorrido> Recorrido); 


public record UltimaDeteccion(
    int ZonaId,                     
    string Zona,
    int Rssi,
    bool Puesta,
    bool Sos,
    bool BateriaBaja,
    DateTime Timestamp,
    bool SenalDebil,                 
    string Metodo,                  
    PosicionEstimada? Posicion);     


public record PosicionEstimada(double X, double Y, double Radio, int Nodos);


public record TramoRecorrido(string Zona, DateTime Desde, DateTime Hasta);
