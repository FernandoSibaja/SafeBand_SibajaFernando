using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;
        

public record CrearLecturaRequest(
    [property: Required(ErrorMessage = "El campo 'nodo' es obligatorio.")]
    string? Nodo,

    [property: Required(ErrorMessage = "El campo 'pulsera' es obligatorio.")]
    string? Pulsera,

    [property: Required(ErrorMessage = "El campo 'rssi' es obligatorio.")]
    [property: Range(-127, 0, ErrorMessage = "El campo 'rssi' debe estar entre -127 y 0 dBm.")]
    int? Rssi,

    bool Puesta = true,
    bool Sos = false,
    bool BateriaBaja = false,


    bool SosCancelado = false,


    DateTimeOffset? Timestamp = null);


public record LecturaResponse(
    long Id,
    string Nodo,
    string Zona,
    string Pulsera,
    string? Alumno,
    int Rssi,
    string Unidad,
    bool Puesta,
    bool Sos,
    bool BateriaBaja,
    DateTime Timestamp);
