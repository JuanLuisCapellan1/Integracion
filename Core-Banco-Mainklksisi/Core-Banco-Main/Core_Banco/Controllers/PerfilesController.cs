using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core_Banco.Data;
using Core_Banco.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegracionBanco.Services;
using System.Text;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todo el controlador
    public class PerfilesController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;

        public PerfilesController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/Perfiles
        [HttpGet]
        [Authorize(Roles = "Admin,Mantenimiento,Usuario")]
        public async Task<ActionResult<IEnumerable<Perfil>>> GetPerfiles()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Perfiles");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                return await _context.Perfiles.ToListAsync();
            }
        }

        // GET: api/Perfiles/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Mantenimiento,Usuario")]
        public async Task<ActionResult<Perfil>> GetPerfil(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Perfiles/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var perfil = await _context.Perfiles.FindAsync(id);

                if (perfil == null)
                {
                    return NotFound();
                }

                return perfil;
            }
        }

        // PUT: api/Perfiles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutPerfil(int id, UpdatePerfilDto updatePerfilDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/Perfiles/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(updatePerfilDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar el perfil en la API del Core." });
                }
            }
            else
            {
                var perfil = await _context.Perfiles.FindAsync(id);
                if (perfil == null)
                {
                    return NotFound();
                }

                perfil.NombrePerfil = updatePerfilDto.NombrePerfil;
                perfil.Descripcion = updatePerfilDto.Descripcion;

                _context.Entry(perfil).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerfilExists(id))
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

        // POST: api/Perfiles
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Perfil>> PostPerfil(CreatePerfilDto createPerfilDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Perfiles")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(createPerfilDto), Encoding.UTF8, "application/json")
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
                    return BadRequest(new { message = "Error al crear el perfil en la API del Core." });
                }
            }
            else
            {
                // Verificar si el PerfilID ya existe
                if (PerfilExists(createPerfilDto.PerfilID))
                {
                    return BadRequest("PerfilID ya existe.");
                }

                var perfil = new Perfil
                {
                    PerfilID = createPerfilDto.PerfilID,
                    NombrePerfil = createPerfilDto.NombrePerfil,
                    Descripcion = createPerfilDto.Descripcion
                };

                _context.Perfiles.Add(perfil);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPerfil", new { id = perfil.PerfilID }, perfil);
            }
        }

        // DELETE: api/Perfiles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePerfil(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/Perfiles/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar el perfil en la API del Core." });
                }
            }
            else
            {
                var perfil = await _context.Perfiles.FindAsync(id);
                if (perfil == null)
                {
                    return NotFound();
                }

                _context.Perfiles.Remove(perfil);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            
        }

        private bool PerfilExists(int id)
        {
            return _context.Perfiles.Any(e => e.PerfilID == id);
        }
    }
}
