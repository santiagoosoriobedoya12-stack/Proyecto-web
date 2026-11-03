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

// Registro para Persona
builder.Services.AddScoped<IRepositorioPersona>(_ => new RepositorioPersonaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioPersona, ServicioPersona>();

// Registro para Rol
builder.Services.AddScoped<IRepositorioRol>(_ => new RepositorioRolSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioRol, ServicioRol>();

// Registro para Ruta
builder.Services.AddScoped<IRepositorioRuta>(_ => new RepositorioRutaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioRuta, ServicioRuta>();

// Registro para Usuario
builder.Services.AddScoped<IRepositorioUsuario>(_ => new RepositorioUsuarioSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();

// ==============================================================
// REGISTROS VERSIÓN 2 (TABLAS CON CLAVE FORÁNEA)
// ==============================================================

// Registro para Cliente (FK Persona y Empresa)
builder.Services.AddScoped<IRepositorioCliente>(_ => new RepositorioClienteSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioCliente, ServicioCliente>();

// Registro para Vendedor (FK Persona)
builder.Services.AddScoped<IRepositorioVendedor>(_ => new RepositorioVendedorSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioVendedor, ServicioVendedor>();

// Registro para Factura (Maestro-Detalle por Procedimientos Almacenados)
builder.Services.AddScoped<IRepositorioFactura>(_ => new RepositorioFacturaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioFactura, ServicioFactura>();

// Registro para Tabla Puente Rol-Usuario
builder.Services.AddScoped<IRepositorioRolUsuario>(_ => new RepositorioRolUsuarioSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioRolUsuario, ServicioRolUsuario>();

// Registro para Tabla Puente Ruta-Rol
builder.Services.AddScoped<IRepositorioRutaRol>(_ => new RepositorioRutaRolSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioRutaRol, ServicioRutaRol>();

// Registro para Recurso Compuesto Usuario con Roles (5 Procedimientos)
builder.Services.AddScoped<IRepositorioUsuarioConRoles>(_ => new RepositorioUsuarioConRolesSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioUsuarioConRoles, ServicioUsuarioConRoles>();

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
        Title = "API Facturación - Versión 2",
        Version = "v2",
        Description = "API REST construida según la metodología por versiones acumulativas (Versión 2: Con Claves Foráneas)."
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

// Endpoint RF7 / Criterio 1: Diagnóstico en la raíz (GET /) que reporta versión v2
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Facturas funcionando",
    version = "v2",
    contratos = "docs/spec_kit/versiones/v2_con_fk/6_contracts.md"
}));

app.MapControllers();

app.Run();

