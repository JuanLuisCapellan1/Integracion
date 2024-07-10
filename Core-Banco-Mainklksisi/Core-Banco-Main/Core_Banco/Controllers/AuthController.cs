using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core_Banco.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Core_Banco.Data;
using IntegracionBanco.Services;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;

        public AuthController(IConfiguration configuration, Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService)
        {
            _configuration = configuration;
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
        }

        [HttpPost("login")]
        public async Task<IActionResult>Login([FromBody] LoginDto loginDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync()) {
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Auth/login")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json")
                };

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    var coreContent = await coreResponse.Content.ReadAsStringAsync();
                    return Content(coreContent, coreResponse.Content.Headers.ContentType?.ToString());
                }
                else
                {
                    return BadRequest(new { message = "Error al autenticar con la API del Core." });
                }
            }
            else
            {
                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest(new { message = "Usuario y contraseña son requeridos." });
                }

                // Buscar el usuario en la base de datos por nombre de usuario
                var user = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loginDto.Username);

                if (user == null)
                {
                    return BadRequest(new { message = "Usuario o contraseña incorrectos." });
                }

                // Verificar que la contraseña no sea nula antes de compararla
                if (user.Contraseña == null || user.Contraseña != loginDto.Password)
                {
                    return BadRequest(new { message = "Usuario o contraseña incorrectos." });
                }

                // Asignar rol basado en el PerfilID del usuario
                string role = user.PerfilID switch
                {
                    1 => "Admin",
                    2 => "User",
                    3 => "Maintenance",
                    _ => null
                };

                if (role == null)
                {
                    return BadRequest(new { message = "Perfil no válido." });
                }

                var token = GenerateJwtToken(user.NombreUsuario, role, user.ClienteID);
                return Ok(new AuthResponseDto { Token = token, ClienteID = user.ClienteID, PerfilID = user.PerfilID });

            }
        }

        private string GenerateJwtToken(string username, string role, int? clienteID)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("ClienteID", clienteID.HasValue ? clienteID.Value.ToString() : string.Empty) // Agrega el ClienteID como un reclamo
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(10), // Duración del token de 10 días
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
