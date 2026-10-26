using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SafeBand.api.Data;
using SafeBand.api.Models;

namespace SafeBand.api.Servicios;

public class TelegramServicio(HttpClient http, IConfiguration config, ILogger<TelegramServicio> log)
{
    public static readonly TimeSpan VigenciaCodigo = TimeSpan.FromMinutes(15);

    // Solo una revisión de mensajes a la vez, y hasta dónde ya se leyó (Telegram los guarda 24 h)
    private static readonly SemaphoreSlim Candado = new(1, 1);
    private static long siguienteUpdate;

    private string? Token => config["Telegram:Token"];
    public string? Bot => config["Telegram:Bot"];
    public bool Disponible => !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrWhiteSpace(Bot);
    public static string NuevoCodigo() =>
        RandomNumberGenerator.GetString("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789", 16);

    public string Enlace(string codigo) => $"https://t.me/{Bot}?start={codigo}";
    public async Task<bool> Enviar(long chatId, string texto)
    {
        if (!Disponible) return false;
        try
        {
            var r = await http.PostAsJsonAsync(Url("sendMessage"), new { chat_id = chatId, text = texto, disable_web_page_preview = true });
            if (r.IsSuccessStatusCode) return true;
            log.LogWarning("Telegram no aceptó el mensaje para el chat {Chat}: HTTP {Estado}", chatId, (int)r.StatusCode);
        }
        catch (Exception e)
        {
            log.LogWarning("No se pudo contactar a Telegram: {Error}", e.GetType().Name);
        }
        return false;
    }
    public async Task RevisarMensajes(AppDbContext db)
    {
        if (!Disponible || !await Candado.WaitAsync(TimeSpan.FromSeconds(5))) return;
        try
        {
            var respuesta = await http.GetFromJsonAsync<Respuesta<List<Update>>>(Url($"getUpdates?offset={siguienteUpdate}&timeout=0"));
            foreach (var u in respuesta?.Result ?? [])
            {
                siguienteUpdate = u.UpdateId + 1;   
                var texto = u.Message?.Text?.Trim();
                var chat = u.Message?.Chat?.Id;
                if (chat is null || texto is null || !texto.StartsWith("/start")) continue;

                var codigo = texto["/start".Length..].Trim();
                var ahora = DateTime.UtcNow;
                var usuario = codigo.Length == 0 ? null
                    : await db.Users.FirstOrDefaultAsync(x => x.TelegramCodigo == codigo && x.TelegramCodigoExpira > ahora && x.Activo);
                if (usuario is null)
                {
                    await Enviar(chat.Value, "Hola, soy el bot de avisos de SafeBand. Para recibir avisos, entra a la página de SafeBand " +
                        "y toca «Recibir avisos por Telegram» (el enlace vence en 15 minutos).");
                    continue;
                }
                usuario.TelegramChatId = chat.Value;
                usuario.TelegramCodigo = null;
                usuario.TelegramCodigoExpira = null;
                await db.SaveChangesAsync();
                await Enviar(chat.Value, $"Listo, {usuario.NombreMostrar}. Te avisaré por aquí si alguno de tus hijos pide ayuda (SOS) en la escuela.");
            }
        }
        catch (Exception e)
        {
            log.LogWarning("No se pudieron revisar los mensajes de Telegram: {Error}", e.GetType().Name);
        }
        finally
        {
            Candado.Release();
        }
    }

    private string Url(string metodo) => $"https://api.telegram.org/bot{Token}/{metodo}";

    private record Respuesta<T>([property: JsonPropertyName("ok")] bool Ok, [property: JsonPropertyName("result")] T? Result);
    private record Update([property: JsonPropertyName("update_id")] long UpdateId, [property: JsonPropertyName("message")] Mensaje? Message);
    private record Mensaje([property: JsonPropertyName("text")] string? Text, [property: JsonPropertyName("chat")] Chat? Chat);
    private record Chat([property: JsonPropertyName("id")] long Id);
}
public static class Avisos
{

    public static async Task Sos(AppDbContext db, TelegramServicio telegram, IConfiguration config, Alerta alerta, string urlPagina)
    {
        if (!telegram.Disponible) return;
        var chats = await db.Users.AsNoTracking()
            .Where(u => u.Activo && u.TelegramChatId != null && u.TutorId != null
                && db.TutoresAlumnos.Any(ta => ta.TutorId == u.TutorId && ta.AlumnoId == alerta.AlumnoId))
            .Select(u => u.TelegramChatId!.Value)
            .ToListAsync();
        if (chats.Count == 0) return;

        var alumno = await db.Alumnos.AsNoTracking().Where(a => a.Id == alerta.AlumnoId).Select(a => a.Nombres).FirstAsync();
        var zona = alerta.ZonaId is null ? null : await db.Zonas.AsNoTracking().Where(z => z.Id == alerta.ZonaId).Select(z => z.Nombre).FirstOrDefaultAsync();
        var escuela = await db.DatosEscuela.AsNoTracking().FirstOrDefaultAsync(d => d.Id == 1);
        var hora = HoraLocal(alerta.Inicio, config).ToString("h:mm tt", new System.Globalization.CultureInfo("es-MX"));

        var texto = $"SafeBand: {alumno} pidió ayuda (SOS) a las {hora}{(zona is null ? "" : $" en {zona}")}. " +
            "La escuela ya recibió el aviso.\n\n" +
            $"Revisa aquí: {urlPagina}" +
            (escuela?.Telefono is { } tel ? $"\nSi lo necesitas, llama a la escuela: {tel}" : "");
        foreach (var chat in chats) await telegram.Enviar(chat, texto);
    }

    private static DateTime HoraLocal(DateTime utc, IConfiguration config)
    {
        var id = config["Escuela:ZonaHoraria"] ?? "America/Tijuana";
        try { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById(id)); }
        catch (TimeZoneNotFoundException) { return utc.AddHours(-7); }
    }
}
