using System.Text;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;
// Contratos e implementaciones de lógica y reglas de negocio
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
// Componentes base de ASP.NET Core MVC (ControllerBase, IActionResult, [HttpGet], [Route])
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================
// 1. CONFIGURACIÓN DE LA BASE DE DATOS Y ENSAMBLADOR (DI)
// ==============================================================
var cadenaConexion = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");

// Registros de la Versión 1 (6 Tablas sin FK)
builder.Services.AddScoped<IRepositorioProducto>(_ => new RepositorioProductoSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioEmpresa>(_ => new RepositorioEmpresaSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioEmpresa, ServicioEmpresa>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioPersona>(_ => new RepositorioPersonaSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioPersona, ServicioPersona>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioRol>(_ => new RepositorioRolSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioRol, ServicioRol>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioRuta>(_ => new RepositorioRutaSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioRuta, ServicioRuta>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioUsuario>(_ => new RepositorioUsuarioSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();

// Registros de la Versión 2 (Tablas con FK y Maestro-Detalle)
builder.Services.AddScoped<IRepositorioCliente>(_ => new RepositorioClienteSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioCliente, ServicioCliente>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioVendedor>(_ => new RepositorioVendedorSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioVendedor, ServicioVendedor>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioFactura>(_ => new RepositorioFacturaSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioFactura, ServicioFactura>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioRolUsuario>(_ => new RepositorioRolUsuarioSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioRolUsuario, ServicioRolUsuario>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioRutaRol>(_ => new RepositorioRutaRolSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioRutaRol, ServicioRutaRol>();

// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioUsuarioConRoles>(_ => new RepositorioUsuarioConRolesSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioUsuarioConRoles, ServicioUsuarioConRoles>();

// Registros de la Versión 3 (Seguridad, Sesión y Control de Acceso RBAC)
builder.Services.Configure<ConfiguracionJwt>(builder.Configuration.GetSection("Jwt"));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IRepositorioAcceso, RepositorioAccesoSqlServer>();
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioSesion, ServicioSesion>();

// Registros de la Versión 4 (Consultas Multitabla y Tablero de Control)
builder.Services.AddScoped<IRepositorioConsultas>(_ => new RepositorioConsultasSqlServer(cadenaConexion));
// Registra el repositorio o servicio en el contenedor de dependencias con ciclo de vida por petición
builder.Services.AddScoped<IServicioConsultas, ServicioConsultas>();

// ==============================================================
// 2. CONFIGURACIÓN DE AUTENTICACIÓN JWT (VERSIÓN 3)
// ==============================================================
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();
// Valida que el identificador o cadena requerida no llegue nula ni vacía
if (string.IsNullOrWhiteSpace(jwtConfig.Key) || jwtConfig.Key.Length < 32)
{
    jwtConfig.Key = "SuperClaveSecretaUltraSeguraParaFirmarJwtDeFacturas2026*";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
        ValidateIssuer = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtConfig.Audience,
        ValidateLifetime = true,
        // Tropiezo 1 evitado (3_plan.md §5): ClockSkew en cero para que un token vencido falle de inmediato
        ClockSkew = TimeSpan.Zero
    };
    // Tropiezo 2 evitado (3_plan.md §5): OnChallenge para que el 401 lleve cuerpo JSON descriptivo
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new
            {
                estado = 401,
                mensaje = "No hay una sesion valida. Inicie sesion."
            });
            await context.Response.WriteAsync(payload);
        }
    };
});

builder.Services.AddAuthorization();

// ==============================================================
// 3. CONTROLADORES Y RESPUESTA 422 PERSONALIZADA
// ==============================================================
builder.Services.AddControllers();
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
// 4. DOCUMENTACIÓN SWAGGER CON SEGURIDAD JWT (Versión 3)
// ==============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Facturación - Versión 3",
        Version = "v3",
        Description = "API REST protegida con JWT Bearer y control de acceso basado en roles (RBAC)."
    });

    // Configuración del candado Authorize en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorización JWT usando el esquema Bearer. Ingrese solo el token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ==============================================================
// 5. PIPELINE DE PETICIONES HTTP
// ==============================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Facturas v4");
    c.RoutePrefix = "swagger";
});

// Tropiezo 3 evitado (3_plan.md §5): UseAuthentication estrictamente ANTES de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Diagnóstico abierto en la raíz (GET /)
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Facturas funcionando",
    version = "v4",
    contratos = "docs/spec_kit/versiones/v4_reportes_dashboard/6_contracts.md"
})).AllowAnonymous();

app.MapControllers();

app.Run();
