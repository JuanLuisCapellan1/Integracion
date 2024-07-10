using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core_Banco.Data;
using Core_Banco.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using IntegracionBanco.Services;
using System.Configuration;
using static Core_Banco.Models.TipoCuenta;
using System.Text;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;
        public UsuariosController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/Usuarios
        [HttpGet]
        [Authorize(Roles = "Admin,User,Maintenance")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Usuarios");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                return await _context.Usuarios.ToListAsync();
            }
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User,Maintenance")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Usuarios/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound();
                }

                return usuario;
            }
        }

        // GET: api/Usuarios/cliente/5
        [HttpGet("cliente/{clienteId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuariosByClienteId(int clienteId)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Usuarios/cliente/{clienteId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var usuarios = await _context.Usuarios.Where(u => u.ClienteID == clienteId).ToListAsync();

                if (usuarios == null || !usuarios.Any())
                {
                    return NotFound();
                }

                return usuarios;
            }
        }

        // POST: api/Usuarios
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Usuario>> PostUsuario(CreateUserDto createUserDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Usuarios")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(createUserDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    var coreContent = await coreResponse.Content.ReadAsStringAsync();
                    return Content(coreContent, coreResponse.Content.Headers.ContentType?.ToString());
                }
                else
                {
                    return BadRequest(new { message = "Error al crear el Usuario en la API del Core." });
                }
            }
            else
            {
                var usuario = new Usuario
                {
                    NombreUsuario = createUserDto.NombreUsuario,
                    Contraseña = createUserDto.Contraseña,
                    PerfilID = createUserDto.PerfilID,
                    ClienteID = createUserDto.ClienteID, // Asigna ClienteID
                    FechaCreacion = DateTime.Now,
                    UltimoAcceso = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioID }, usuario);
            }
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUsuario(int id, UpdateUserDto updateUserDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/Usuarios/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(updateUserDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar el Usuario en la API del Core." });
                }
            }
            else
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.NombreUsuario = updateUserDto.NombreUsuario;
                usuario.Contraseña = updateUserDto.Contraseña;
                usuario.PerfilID = updateUserDto.PerfilID;
                usuario.ClienteID = updateUserDto.ClienteID; // Asigna ClienteID

                _context.Entry(usuario).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/Usuarios/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar el Usuario en la API del Core." });
                }
            }
            else
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioID == id);
        }
    }
}
