using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiFacturas.Servicios;

/// <summary>
/// Servicio de dominio para autenticación y emisión de tokens JWT (Versión 3).
/// Valida la contraseña mediante hash BCrypt y genera un token sin estado que transporta correo y roles.
/// </summary>
public class ServicioSesion : IServicioSesion
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioAcceso _repositorioAcceso;
    private readonly ConfiguracionJwt _jwtConfig;

    public ServicioSesion(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioAcceso repositorioAcceso,
        IOptions<ConfiguracionJwt> jwtConfig)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioAcceso = repositorioAcceso;
        _jwtConfig = jwtConfig.Value;
    }

    /// <inheritdoc />
    public async Task<Sesion?> IniciarSesionAsync(SesionCrear peticion)
    {
        // 1. Buscar usuario por email
        var usuario = await _repositorioUsuario.ObtenerPorEmailAsync(peticion.Email);
        if (usuario == null)
        {
            // Criterio 2: Retornar null para emitir exactamente el mismo 401 que con clave errada
            return null;
        }

        // 2. Comprobar la contraseña
        bool esValida = false;
        try
        {
            if (usuario.Contrasena.StartsWith("$2a$") || usuario.Contrasena.StartsWith("$2b$"))
            {
                // Validación con BCrypt (costo 12)
                esValida = BCrypt.Net.BCrypt.Verify(peticion.Contrasena, usuario.Contrasena);
            }
            else
            {
                // Soporte de transición resiliente si la contraseña estaba en texto plano:
                // La valida y actualiza automáticamente al hash BCrypt costo 12
                esValida = (peticion.Contrasena == usuario.Contrasena);
                if (esValida)
                {
                    var nuevoHash = BCrypt.Net.BCrypt.HashPassword(peticion.Contrasena, 12);
                    usuario.Contrasena = nuevoHash;
                    _ = _repositorioUsuario.ActualizarAsync(usuario.Email, new UsuarioActualizar { Contrasena = nuevoHash });
                }
            }
        }
        catch
        {
            esValida = (peticion.Contrasena == usuario.Contrasena);
        }

        if (!esValida)
        {
            return null;
        }

        // 3. Obtener los nombres de los roles asignados
        var roles = await _repositorioAcceso.ObtenerRolesPorEmailAsync(usuario.Email);

        // 4. Configurar claims del token JWT
        // NOTA DE ARQUITECTURA (3_plan.md / 4_research.md D4):
        // En el token se guardan ÚNICAMENTE el correo y los nombres de los roles.
        // NUNCA se guardan los permisos de ruta en el token, para que quitar un permiso surta efecto inmediato.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
            claims.Add(new Claim("role", rol));
        }

        // 5. Firmar el token JWT
        if (string.IsNullOrWhiteSpace(_jwtConfig.Key) || _jwtConfig.Key.Length < 32)
        {
            throw new InvalidOperationException("La clave Jwt:Key debe tener al menos 32 caracteres (256 bits).");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_jwtConfig.Key);
        var claveSimetrica = new SymmetricSecurityKey(keyBytes);
        var credencialesFirma = new SigningCredentials(claveSimetrica, SecurityAlgorithms.HmacSha256);
        var fechaExpiracion = DateTime.UtcNow.AddHours(_jwtConfig.ExpiracionHoras > 0 ? _jwtConfig.ExpiracionHoras : 1);

        var tokenJwt = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: fechaExpiracion,
            signingCredentials: credencialesFirma
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenJwt);

        return new Sesion
        {
            Estado = 200,
            Mensaje = "Sesion iniciada.",
            Token = tokenString,
            Email = usuario.Email,
            Roles = roles,
            Expira = fechaExpiracion
        };
    }
}

