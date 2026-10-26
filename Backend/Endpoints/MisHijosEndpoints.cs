using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeBand.api.Contracts;
using SafeBand.api.Data;
using SafeBand.api.Models;
using SafeBand.api.Servicios;

namespace SafeBand.api.Endpoints;


public static class MisHijosEndpoints
{

    private static readonly TimeSpan HuecoMaximo = TimeSpan.FromMinutes(10);
    private const int MaxLecturasRecorrido = 5000;

    public static void MapMisHijos(this WebApplication app)
    {
        app.MapGet("/api/mis-hijos", MisHijos)
            .RequireAuthorization(Politicas.SoloPadre)
            .WithTags("Familia")
            .WithSummary("Los hijos del padre con sesión: dónde están y su recorrido desde ?desde= (por defecto, las últimas 12 h)");
    }


    private static async Task<IResult> MisHijos(
        ClaimsPrincipal yo, UserManager<Usuario> usuarios, AppDbContext db, IConfiguration config, DateTimeOffset? desde)
    {
        var usuario = await usuarios.GetUserAsync(yo);
        if (usuario?.TutorId is null) return Results.Ok(new List<MiHijoResponse>());


        var desdeUtc = desde?.UtcDateTime ?? DateTime.UtcNow.AddHours(-12);

        var hijos = await db.TutoresAlumnos.AsNoTracking()
            .Where(ta => ta.TutorId == usuario.TutorId && ta.Alumno.Activo)
            .OrderBy(ta => ta.Alumno.Nombres)
            .Select(ta => new
            {
                ta.AlumnoId,
                Nombre = ta.Alumno.Nombres + " " + ta.Alumno.ApellidoPaterno,
                ta.Parentesco,
                Pulsera = ta.Alumno.Pulseras.Where(p => p.Activa).Select(p => p.IdentificadorBle).FirstOrDefault(),
            })
            .ToListAsync();

        var ctx = await Detecciones.CargarContexto(db, config);

        var resultado = new List<MiHijoResponse>();
        foreach (var h in hijos)
        {
            var ultima = await Detecciones.Ultima(db, h.AlumnoId, ctx);
            var recorrido = ArmarRecorrido(await Detecciones.ZonasPorMomento(db, h.AlumnoId, desdeUtc, MaxLecturasRecorrido, ctx));
            resultado.Add(new MiHijoResponse(h.AlumnoId, h.Nombre, h.Parentesco, h.Pulsera, ultima, recorrido));
        }
        return Results.Ok(resultado);
    }

    private static List<TramoRecorrido> ArmarRecorrido(IEnumerable<(string Zona, DateTime Hora)> lecturas)
    {
        var tramos = new List<TramoRecorrido>();
        foreach (var (zona, hora) in lecturas)
        {
            var ultimo = tramos.Count > 0 ? tramos[^1] : null;
            if (ultimo is not null && ultimo.Zona == zona && hora - ultimo.Hasta <= HuecoMaximo)
                tramos[^1] = ultimo with { Hasta = hora };
            else
                tramos.Add(new TramoRecorrido(zona, hora, hora));
        }
        return tramos;
    }
}
