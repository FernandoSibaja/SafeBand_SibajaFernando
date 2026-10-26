using System.ComponentModel.DataAnnotations;

namespace SafeBand.api.Contracts;


public record GuardarPulseraRequest(

    [property: Required(ErrorMessage = "El campo 'identificadorBle' es obligatorio.")]
    [property: RegularExpression(@"^[Ss][Bb]-[A-Za-z0-9]{1,12}$",
        ErrorMessage = "El campo 'identificadorBle' debe tener el formato SB-XXXX (letras o números, máximo 15 caracteres).")]
    string? IdentificadorBle,


    [property: StringLength(40, ErrorMessage = "El campo 'uidNfc' es demasiado largo.")]
    string? UidNfc = null,

    int? AlumnoId = null);


public record AsignarPulseraRequest(int? AlumnoId);


public record PulseraResponse(
    int Id,
    string IdentificadorBle,
    string? UidNfc,
    bool Activa,
    DateTime FechaAlta,
    DateTime? UltimaLectura,
    int? AlumnoId,
    string? Alumno);   
