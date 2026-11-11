using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrontFacturas.Modelos.Consultas;

namespace FrontFacturas.Servicios;

/// <summary>
/// Cliente de comunicación HTTP entre Blazor Server y la API REST para los módulos de la Versión 4.
/// 
/// REGLAS CLAVE:
/// 1. Es el único componente del frontend que conoce los endpoints de '/api/consultas' y '/api/dashboard'.
/// 2. Inyecta automáticamente el token Bearer en el encabezado 'Authorization' antes de cada llamada HTTP.
/// 3. Devuelve colecciones tipadas hacia las páginas Razor ('Dashboard.razor' y 'Consultas.razor').
/// </summary>
public class ServicioConsultas
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;

    /// <summary>
    /// Constructor: Inyecta el HttpClient configurado en Program.cs y el EstadoSesion del usuario actual.
    /// </summary>
    public ServicioConsultas(HttpClient http, EstadoSesion estadoSesion)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _estadoSesion = estadoSesion ?? throw new ArgumentNullException(nameof(estadoSesion));
    }

    /// <summary>
    /// Inyecta el token JWT Bearer si la sesión está autenticada, evitando el error 401 Unauthorized.
    /// </summary>
    private void Autorizar()
    {
        if (_estadoSesion.EstaAutenticado)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _estadoSesion.Token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// Obtiene los indicadores gerenciales (KPIs) del Tablero de Control llamando a GET api/dashboard/kpis.
    /// </summary>
    public async Task<DashboardKpisDto?> ObtenerDashboardKpisAsync()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<DashboardKpisDto>("api/dashboard/kpis");
    }

    /// <summary>
    /// Ejecuta la Consulta 1: Consolidado Maestro de Facturación (6 tablas).
    /// </summary>
    public async Task<List<ConsultaFacturaDetalladaDto>> ObtenerC1Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaFacturaDetalladaDto>>("api/consultas/c1") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 2: Ranking de Productos más Vendidos (4 tablas).
    /// </summary>
    public async Task<List<ConsultaRankingProductosDto>> ObtenerC2Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaRankingProductosDto>>("api/consultas/c2") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 3: Desempeño Comercial por Vendedor (5 tablas).
    /// </summary>
    public async Task<List<ConsultaRendimientoVendedoresDto>> ObtenerC3Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaRendimientoVendedoresDto>>("api/consultas/c3") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 4: Clientes de Alto Valor / Clientes VIP (5 tablas).
    /// </summary>
    public async Task<List<ConsultaClientesAltoValorDto>> ObtenerC4Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaClientesAltoValorDto>>("api/consultas/c4") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 5: Alerta Preventiva de Stock Crítico (4 tablas).
    /// </summary>
    public async Task<List<ConsultaStockCriticoDto>> ObtenerC5Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaStockCriticoDto>>("api/consultas/c5") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 6: Auditoría de Seguridad y Roles (5 tablas).
    /// </summary>
    public async Task<List<ConsultaAuditoriaSeguridadDto>> ObtenerC6Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaAuditoriaSeguridadDto>>("api/consultas/c6") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 7: Renglones Detallados de Factura (5 tablas).
    /// </summary>
    public async Task<List<ConsultaRenglonesFacturaDto>> ObtenerC7Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaRenglonesFacturaDto>>("api/consultas/c7") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 8: Cartera de Crédito y Exposición Financiera (4 tablas).
    /// </summary>
    public async Task<List<ConsultaCarteraCreditoDto>> ObtenerC8Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaCarteraCreditoDto>>("api/consultas/c8") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 9: Detección de Clientes Inactivos (4 tablas).
    /// </summary>
    public async Task<List<ConsultaClientesInactivosDto>> ObtenerC9Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaClientesInactivosDto>>("api/consultas/c9") ?? new();
    }

    /// <summary>
    /// Ejecuta la Consulta 10: Auditoría de Facturas Anuladas (5 tablas).
    /// </summary>
    public async Task<List<ConsultaFacturasAnuladasDto>> ObtenerC10Async()
    {
        Autorizar();
        return await _http.GetFromJsonAsync<List<ConsultaFacturasAnuladasDto>>("api/consultas/c10") ?? new();
    }
}
