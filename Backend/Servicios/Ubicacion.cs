using SafeBand.api.Models;

namespace SafeBand.api.Servicios;
public static class Ubicacion
{

    public static readonly TimeSpan Simultaneas = TimeSpan.FromSeconds(15);

    public record Lectura(DateTime Hora, int ZonaId, string Zona, int Rssi, int UmbralRssi, bool Puesta, bool Sos, bool BateriaBaja,
        int NodoId = 0, double? NodoX = null, double? NodoY = null);

    public record Calibracion(double Rssi1m, double N);

    public record Posicion(double X, double Y, double Radio, int Nodos);

    private const double DistanciaMin = 0.3, DistanciaMax = 30, RadioMin = 1.5, RadioMax = 12;

    private const double MargenDb = 12;

    private const double HolguraFuerte = 2;

    public static double Distancia(double rssi, Calibracion c) =>
        Math.Clamp(Math.Pow(10, (c.Rssi1m - rssi) / (10 * c.N)), DistanciaMin, DistanciaMax);

    public static Posicion? Estimar(IEnumerable<Lectura> grupo, Calibracion c, double? anchoPlano = null, double? altoPlano = null)
    {
        var medidas = grupo.Where(l => l.NodoX is not null && l.NodoY is not null)
            .GroupBy(l => l.NodoId)
            .Select(g => (X: g.First().NodoX!.Value, Y: g.First().NodoY!.Value, Rssi: g.Average(l => l.Rssi)))
            .OrderByDescending(m => m.Rssi)
            .ToList();
        if (medidas.Count == 0) return null;

        var nodos = medidas.Where(m => m.Rssi >= medidas[0].Rssi - MargenDb)
            .Select(m => (m.X, m.Y, D: Distancia(m.Rssi, c)))
            .ToList();

        double x, y, radio;
        if (nodos.Count == 1)
        {
            (x, y, radio) = (nodos[0].X, nodos[0].Y, nodos[0].D);
        }
        else if (nodos.Count == 2)
        {
            var (a, b) = (nodos[0], nodos[1]);
            var separacion = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
            var t = a.D / (a.D + b.D);                      
            (x, y) = (a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));
            radio = Math.Abs(separacion - (a.D + b.D)) / 2 + 2.5;   
        }
        else
        {
            
            double sw = 0;
            x = y = 0;
            foreach (var n in nodos) { var w = 1 / (n.D * n.D); x += w * n.X; y += w * n.Y; sw += w; }
            x /= sw; y /= sw;
            for (var i = 0; i < 30; i++)
            {
                double a11 = 0, a12 = 0, a22 = 0, b1 = 0, b2 = 0;
                foreach (var n in nodos)
                {
                    var dx = x - n.X; var dy = y - n.Y;
                    var dist = Math.Max(Math.Sqrt(dx * dx + dy * dy), 1e-6);
                    var (jx, jy) = (dx / dist, dy / dist);
                    var r = dist - n.D;
                    var w = 1 / (n.D * n.D);
                    a11 += w * jx * jx; a12 += w * jx * jy; a22 += w * jy * jy;
                    b1 += w * jx * r; b2 += w * jy * r;
                }
                var det = a11 * a22 - a12 * a12;
                if (Math.Abs(det) < 1e-12) break;          
                var px = (a22 * b1 - a12 * b2) / det;
                var py = (a11 * b2 - a12 * b1) / det;
                x -= px; y -= py;
                if (px * px + py * py < 1e-6) break;
            }
            double suma = 0, pesos = 0;
            foreach (var n in nodos)
            {
                var w = 1 / (n.D * n.D);
                var r = Math.Sqrt(Math.Pow(x - n.X, 2) + Math.Pow(y - n.Y, 2)) - n.D;
                suma += w * r * r; pesos += w;
            }
            radio = Math.Sqrt(suma / pesos) + RadioMin;
        }
        var fuerte = nodos[0];
        var limite = fuerte.D + HolguraFuerte;
        var lejania = Math.Sqrt(Math.Pow(x - fuerte.X, 2) + Math.Pow(y - fuerte.Y, 2));
        if (lejania > limite)
        {
            x = fuerte.X + (x - fuerte.X) * limite / lejania;
            y = fuerte.Y + (y - fuerte.Y) * limite / lejania;
            radio = Math.Max(radio, limite / 2);
        }

        if (anchoPlano is > 0) x = Math.Clamp(x, 0, anchoPlano.Value);
        if (altoPlano is > 0) y = Math.Clamp(y, 0, altoPlano.Value);
        return new Posicion(Math.Round(x, 1), Math.Round(y, 1), Math.Round(Math.Clamp(radio, RadioMin, RadioMax), 1), nodos.Count);
    }


    public record ZonaInfo(int Id, string Nombre, ModoUbicacion Modo, bool TieneNodos,
        double? X = null, double? Y = null, double? Ancho = null, double? Alto = null);

    public record Decision(int ZonaId, string Zona, string Metodo, bool Cerca, Posicion? Posicion);

    private const double RadioConfiable = 6;


    public static Decision Decidir(IReadOnlyCollection<Lectura> grupo, IReadOnlyDictionary<int, ZonaInfo> zonas,
        Calibracion c, double? anchoPlano = null, double? altoPlano = null)
    {
        var fuerte = MasFuerte(grupo);
        var p = Estimar(grupo, c, anchoPlano, altoPlano);
        if (EsDebil(fuerte))
            return new Decision(fuerte.ZonaId, fuerte.Zona, "Cercania", true, p);

        var modo = zonas.TryGetValue(fuerte.ZonaId, out var zf) ? zf.Modo : ModoUbicacion.PorNodo;
        if (modo == ModoUbicacion.PorNodo)
            return new Decision(fuerte.ZonaId, fuerte.Zona, "Nodo", false, p);

        if (p is not null && p.Radio <= RadioConfiable && ZonaDelPunto(p, zonas.Values) is { } z)
            return new Decision(z.Id, z.Nombre, "Triangulacion", false, p);

        return new Decision(fuerte.ZonaId, fuerte.Zona, "Nodo", false, p);
    }

    public static ZonaInfo? ZonaDelPunto(Posicion p, IEnumerable<ZonaInfo> zonas)
    {
        var candidatas = zonas.Where(z => z.X is not null && !(z.Modo == ModoUbicacion.PorNodo && z.TieneNodos)).ToList();
        double Lejania(ZonaInfo z)
        {
            var dx = Math.Max(Math.Max(z.X!.Value - p.X, 0), p.X - (z.X.Value + z.Ancho!.Value));
            var dy = Math.Max(Math.Max(z.Y!.Value - p.Y, 0), p.Y - (z.Y.Value + z.Alto!.Value));
            return Math.Sqrt(dx * dx + dy * dy);
        }

        var dentro = candidatas.Where(z => Lejania(z) == 0).MinBy(z => z.Ancho!.Value * z.Alto!.Value);
        if (dentro is not null) return dentro;
        var cercana = candidatas.MinBy(Lejania);
        return cercana is not null && Lejania(cercana) <= p.Radio ? cercana : null;
    }

    public static Lectura MasFuerte(IReadOnlyCollection<Lectura> grupo)
    {
        var fuerte = grupo.OrderByDescending(l => l.Rssi).ThenByDescending(l => l.Hora).First();
        var reciente = grupo.MaxBy(l => l.Hora)!;
        return fuerte with
        {
            Hora = reciente.Hora,
            Puesta = reciente.Puesta,
            Sos = grupo.Any(l => l.Sos),
            BateriaBaja = grupo.Any(l => l.BateriaBaja),
        };
    }

    public static bool EsDebil(Lectura l) => l.Rssi < l.UmbralRssi;

    public static List<List<Lectura>> Bloques(IEnumerable<Lectura> lecturasOrdenadas)
    {
        var resultado = new List<List<Lectura>>();
        var bloque = new List<Lectura>();
        foreach (var l in lecturasOrdenadas)
        {
            if (bloque.Count > 0 && l.Hora - bloque[0].Hora > Simultaneas)
            {
                resultado.Add(bloque);
                bloque = [];
            }
            bloque.Add(l);
        }
        if (bloque.Count > 0) resultado.Add(bloque);
        return resultado;
    }
    public static List<Lectura> PorMomento(IEnumerable<Lectura> lecturasOrdenadas) =>
        Bloques(lecturasOrdenadas).Select(MasFuerte).ToList();
}
