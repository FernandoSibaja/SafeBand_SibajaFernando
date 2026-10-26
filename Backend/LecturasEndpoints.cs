using Microsoft.EntityFrameworkCore;
using SafeBand.api.Contracts;
using SafeBand.api.Data;
using SafeBand.api.Models;
using SafeBand.api.Servicios;

namespace SafeBand.api.Endpoints;


public static class LecturasEndpoints
{
    public static void MapLecturas(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/lecturas").WithTags("Lecturas BLE");


        grupo.MapPost("/", Crear).AllowAnonymous().WithSummary("Guarda una lectura enviada por un nodo ESP32");
        grupo.MapGet("/", Listar).RequireAuthorization(Politicas.SoloAdministrador)   
            .WithSummary("Últimas lecturas de toda la escuela (?limit=1 = la más reciente). Solo Administrador");
    }

    // POST /api/lecturas
    private static async Task<IResult> Crear(CrearLecturaRequest req, AppDbContext db, HttpRequest http,
        TelegramServicio telegram, IConfiguration config)
    {

        var nodo = await db.Nodos.Include(n => n.Zona).FirstOrDefaultAsync(n => n.Codigo == req.Nodo);
        if (nodo is null || !nodo.Activo)
            return Results.BadRequest(new { error = $"El nodo '{req.Nodo}' no está registrado o está inactivo." });

        var pulsera = await db.Pulseras.Include(p => p.Alumno).FirstOrDefaultAsync(p => p.IdentificadorBle == req.Pulsera);
        if (pulsera is null || !pulsera.Activa)
            return Results.BadRequest(new { error = $"La pulsera '{req.Pulsera}' no está registrada o está inactiva." });


        var ahora = DateTime.UtcNow;
        var hora = req.Timestamp?.UtcDateTime ?? ahora;
        if (hora > ahora.AddMinutes(5))
            return Results.BadRequest(new { error = "El campo 'timestamp' no puede estar en el futuro." });


        var lectura = new LecturaBle
        {
            NodoId = nodo.Id,
            PulseraId = pulsera.Id,
            AlumnoId = pulsera.AlumnoId,   /
            Rssi = req.Rssi!.Value,
            Puesta = req.Puesta,
            Sos = req.Sos,
            BateriaBaja = req.BateriaBaja,
            Timestamp = hora
        };
        db.LecturasBle.Add(lectura);


        nodo.UltimoContacto = ahora;
        pulsera.UltimaLectura = hora;


        Alerta? sosNuevo = null;
        if (pulsera.AlumnoId is int alumnoId)
            sosNuevo = await Alertas.ProcesarLectura(db, alumnoId, nodo.ZonaId, req.Sos, req.SosCancelado, ahora);

        await db.SaveChangesAsync();

        if (sosNuevo is not null)
            await Avisos.Sos(db, telegram, config, sosNuevo, $"{http.Scheme}://{http.Host}/");


        var respuesta = new LecturaResponse(
            lectura.Id, nodo.Codigo, nodo.Zona.Nombre, pulsera.IdentificadorBle,
            NombreCompleto(pulsera.Alumno), lectura.Rssi, "dBm",
            lectura.Puesta, lectura.Sos, lectura.BateriaBaja, lectura.Timestamp);

        return Results.Created($"/api/lecturas/{lectura.Id}", respuesta);
    }


    private static async Task<IResult> Listar(AppDbContext db, int? limit)
    {
        var cantidad = limit ?? 100;
        if (cantidad < 1 || cantidad > 1000)
            return Results.BadRequest(new { error = "El parámetro 'limit' debe estar entre 1 y 1000." });

        var lecturas = await db.LecturasBle
            .AsNoTracking()                         
            .OrderByDescending(l => l.Timestamp)     
            .Take(cantidad)
            .Select(l => new LecturaResponse(
                l.Id, l.Nodo.Codigo, l.Nodo.Zona.Nombre, l.Pulsera.IdentificadorBle,
                l.Alumno == null ? null : l.Alumno.Nombres + " " + l.Alumno.ApellidoPaterno,
                l.Rssi, "dBm", l.Puesta, l.Sos, l.BateriaBaja,
                DateTime.SpecifyKind(l.Timestamp, DateTimeKind.Utc)))  
            .ToListAsync();

        return Results.Ok(lecturas);
    }

    private static string? NombreCompleto(Alumno? a) =>
        a is null ? null : $"{a.Nombres} {a.ApellidoPaterno}";
}
