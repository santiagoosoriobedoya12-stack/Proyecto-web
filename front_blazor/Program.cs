using FrontFacturas.Components;
using FrontFacturas.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================
// 1. CONFIGURACIÓN DE COMPONENTES BLAZOR (Interactive Server)
// ==============================================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ==============================================================
// 2. REGISTRO DEL CLIENTE HTTP PARA ServicioProducto
// ==============================================================
// Lee 'UrlApi' de appsettings.json o de la variable de entorno de Docker Compose:
//   En desarrollo local: http://localhost:8032
//   En contenedor Docker: http://api-facturas:8032
var urlApi = builder.Configuration["UrlApi"] ?? "http://localhost:8032";

// Registramos el servicio tipado con HttpClient inyectado:
// 'ServicioProducto' es el ÚNICO lugar del frontend que consume la API
builder.Services.AddHttpClient<ServicioProducto>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// Registro para ServicioEmpresa
builder.Services.AddHttpClient<ServicioEmpresa>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// Registro para ServicioPersona
builder.Services.AddHttpClient<ServicioPersona>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// Registro para ServicioRol
builder.Services.AddHttpClient<ServicioRol>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// Registro para ServicioRuta
builder.Services.AddHttpClient<ServicioRuta>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// Registro para ServicioUsuario
builder.Services.AddHttpClient<ServicioUsuario>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// ==============================================================
// REGISTROS VERSIÓN 2 (TABLAS CON CLAVE FORÁNEA)
// ==============================================================

builder.Services.AddHttpClient<ServicioCliente>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<ServicioVendedor>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<ServicioFactura>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<ServicioRolUsuario>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<ServicioRutaRol>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<ServicioUsuarioConRoles>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

// ==============================================================
// REGISTROS VERSIÓN 3 (SEGURIDAD, SESIÓN Y CONTROL DE ACCESO)
// ==============================================================
// EstadoSesion es SCOPED: uno por circuito de usuario conectado (3_plan.md §4.1)
builder.Services.AddScoped<EstadoSesion>();

// ServicioSesion para consumir POST /api/sesion
builder.Services.AddHttpClient<ServicioSesion>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi.TrimEnd('/') + "/");
});

var app = builder.Build();

// ==============================================================
// 3. PIPELINE DE LA APLICACIÓN WEB
// ==============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Habilita servir archivos estáticos desde wwwroot (CSS local, etc.)
app.UseStaticFiles();
app.UseAntiforgery();

// Mapea el componente raíz 'App' con modo interactivo en servidor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

