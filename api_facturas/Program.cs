using ApiFacturas.Repositorios;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================
// 1. CONFIGURACIÓN DE LA BASE DE DATOS Y ENSAMBLADOR (DI)
// ==============================================================
// Obtenemos la cadena de conexión desde appsettings.json o desde la
// variable de entorno 'ConnectionStrings__SqlServer' inyectada por Docker.
var cadenaConexion = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");

// Registramos el Repositorio pasando la cadena de conexión concreta.
// El servicio dependerá únicamente de la interfaz IRepositorioProducto.
builder.Services.AddScoped<IRepositorioProducto>(_ => new RepositorioProductoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();

// Registro para Empresa
builder.Services.AddScoped<IRepositorioEmpresa>(_ => new RepositorioEmpresaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioEmpresa, ServicioEmpresa>();

// ==============================================================
// 2. CONFIGURACIÓN DE CONTROLADORES Y RESPUESTA 422 PERSONALIZADA
// ==============================================================
builder.Services.AddControllers();

// Personalizamos el comportamiento de validación de ModelState:
// Cuando una petición no cumple las anotaciones [Required], [Range], etc.,
// ASP.NET por defecto retorna 400. Aquí lo configuramos para retornar exactamente
// el formato exigido por la especificación: HTTP 422 con {estado, mensaje, errores:[...]}.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var listaErrores = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? "El formato o tipo de dato enviado es inválido."
                : e.ErrorMessage)
            .ToList();

        var resultado = new
        {
            estado = 422,
            mensaje = "Datos inválidos.",
            errores = listaErrores
        };

        return new UnprocessableEntityObjectResult(resultado);
    };
});

// ==============================================================
// 3. DOCUMENTACIÓN INTERACTIVA SWAGGER (Swashbuckle)
// ==============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Facturación - Versión 1",
        Version = "v1",
        Description = "API REST construida según la metodología por versiones acumulativas."
    });
});

var app = builder.Build();

// ==============================================================
// 4. PIPELINE DE PETICIONES HTTP
// ==============================================================

// Habilitamos Swagger tanto en desarrollo como en producción didáctica
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Facturas v1");
    c.RoutePrefix = "swagger"; // Disponible en http://localhost:8032/swagger
});

// Endpoint RF7: Diagnóstico en la raíz (GET /)
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Facturas funcionando",
    version = "v1",
    contratos = "docs/spec_kit/versiones/v1_sin_fk/6_contracts.md"
}));

app.MapControllers();

app.Run();

