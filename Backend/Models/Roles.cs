namespace SafeBand.api.Models;

public static class Roles
{
    public const string Administrador = "Administrador";   // ve y administra todo
    public const string Maestro = "Maestro";               // ve a su grupo, pase de lista
    public const string Padre = "Padre";                   // ve solo a sus hijos

    public static readonly string[] Todos = [Administrador, Maestro, Padre];
}

public static class Politicas
{
    public const string SoloAdministrador = "SoloAdministrador";
    public const string Personal = "Personal";       // Administrador o Maestro (personal de la escuela)
    public const string SoloPadre = "SoloPadre";
    public const string SoloMaestro = "SoloMaestro";
}
