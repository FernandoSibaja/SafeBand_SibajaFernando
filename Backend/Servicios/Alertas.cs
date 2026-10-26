using Microsoft.EntityFrameworkCore;
using SafeBand.api.Data;
using SafeBand.api.Models;

namespace SafeBand.api.Servicios;

public static class Alertas
{

    public static async Task<Alerta?> ProcesarLectura(AppDbContext db, int alumnoId, int zonaId, bool sos, bool sosCancelado, DateTime ahora)
    {
        var abierta = await db.Alertas.FirstOrDefaultAsync(a => a.AlumnoId == alumnoId && a.Tipo == TipoAlerta.Sos && a.Fin == null);

        if (sosCancelado)
        {
            if (abierta is not null)
            {
                abierta.Fin = ahora;
                abierta.DesactivadaDesde = OrigenDesactivacion.Pulsera;
            }
            return null;
        }

        if (!sos || abierta is not null) return null;

        var ultimoFin = await db.Alertas
            .Where(a => a.AlumnoId == alumnoId && a.Tipo == TipoAlerta.Sos && a.Fin != null)
            .MaxAsync(a => a.Fin);
        if (ultimoFin is not null
            && !await db.LecturasBle.AnyAsync(l => l.AlumnoId == alumnoId && l.Timestamp > ultimoFin && !l.Sos))
            return null;

        var nueva = new Alerta { Tipo = TipoAlerta.Sos, AlumnoId = alumnoId, ZonaId = zonaId, Inicio = ahora };
        db.Alertas.Add(nueva);
        return nueva;
    }

    public static async Task<HashSet<int>> ConSosActivo(AppDbContext db) =>
        (await db.Alertas.AsNoTracking()
            .Where(a => a.Tipo == TipoAlerta.Sos && a.Fin == null)
            .Select(a => a.AlumnoId)
            .ToListAsync())
        .ToHashSet();
}
