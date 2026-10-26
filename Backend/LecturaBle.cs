namespace SafeBand.api.Models;


public class LecturaBle
{

    public long Id { get; set; }


    public int NodoId { get; set; }
    public Nodo Nodo { get; set; } = null!;


    public int PulseraId { get; set; }
    public Pulsera Pulsera { get; set; } = null!;

    public int? AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }


    public int Rssi { get; set; }


    public bool Puesta { get; set; } = true;   
    public bool Sos { get; set; }              
    public bool BateriaBaja { get; set; }     

    public DateTime Timestamp { get; set; }
}
