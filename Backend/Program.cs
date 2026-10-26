using SafeBand.api.Servicios;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeBand.api.Data;
using SafeBand.api.Endpoints;
using SafeBand.api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SafeBand"),
        sql => sql.EnableRetryOnFailure()));


builder.Services.AddIdentityCore<Usuario>(o =>
    {
        o.Password.RequiredLength = 8;
        o.Password.RequireUppercase = true;
        o.Password.RequireLowercase = true;
        o.Password.RequireDigit = true;
        o.Password.RequireNonAlphanumeric = false;

        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        o.Lockout.AllowedForNewUsers = true;

        o.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()                       
    .AddErrorDescriber<ErroresIdentityEs>();  


builder.Services.AddDataProtection()
    .SetApplicationName("SafeBand")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "claves")));


builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "SafeBand.Sesion";
    o.Cookie.HttpOnly = true;                      
    o.Cookie.SameSite = SameSiteMode.Strict;       
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  
    o.ExpireTimeSpan = TimeSpan.FromDays(7);
    o.SlidingExpiration = true;                     

    
    o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Politicas.SoloAdministrador, p => p.RequireRole(Roles.Administrador))
    .AddPolicy(Politicas.Personal, p => p.RequireRole(Roles.Administrador, Roles.Maestro))
    .AddPolicy(Politicas.SoloPadre, p => p.RequireRole(Roles.Padre))
    .AddPolicy(Politicas.SoloMaestro, p => p.RequireRole(Roles.Maestro));


builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    o.OnRejected = async (contexto, cancelacion) =>
    {
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Demasiados intentos. Espera un minuto e intenta de nuevo." }, cancelacion);
    };
});


builder.Services.AddHttpClient<TelegramServicio>(c => c.Timeout = TimeSpan.FromSeconds(8));


builder.Services.AddProblemDetails();
builder.Services.AddValidation();


builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();


app.UseExceptionHandler(e => e.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ex is BadHttpRequestException bad)
    {
        var campo = (bad.InnerException as JsonException)?.Path?.TrimStart('$', '.');
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(new
        {
            error = string.IsNullOrEmpty(campo)
                ? "El cuerpo de la petición no es un JSON válido."
                : $"El campo '{campo}' tiene un valor o tipo de dato inválido."
        });
        return;
    }
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await ctx.Response.WriteAsJsonAsync(new { error = "Error interno del servidor." });
}));


app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "SafeBand API"));


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SembrarAsync(db);
    await DbSeeder.SembrarUsuariosAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}


app.UseDefaultFiles();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache",
});


app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();



app.MapAuth();       
app.MapLecturas(); 
app.MapAlumnos();    
app.MapGrupos();    
app.MapPersonal();   
app.MapPulseras();  
app.MapTutores();    
app.MapZonasNodos(); 
app.MapMisHijos();  
app.MapMiGrupo();    
app.MapEscuela();    
app.MapAlertas();  
app.MapDatosEscuela(); 
app.MapTelegram();   
app.Run();
