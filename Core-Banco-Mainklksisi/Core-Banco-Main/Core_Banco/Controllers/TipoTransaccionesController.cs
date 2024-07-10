using Core_Banco.Data;
using Core_Banco.Models;
using IntegracionBanco.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core_Banco.Models.TipoCuenta;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todo el controlador
    public class TipoTransaccionesController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;
        public TipoTransaccionesController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/TipoTransacciones
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<TipoTransaccionDto>>> GetTipoTransacciones()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/TipoTransacciones");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var tipos = await _context.TiposTransaccion
                .Select(t => new TipoTransaccionDto
                {
                    TipoTransaccionID = t.TipoTransaccionID,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

                return Ok(tipos);
            }
        }

        // GET: api/TipoTransacciones/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<TipoTransaccionDto>> GetTipoTransaccion(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/TipoTransacciones/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var tipo = await _context.TiposTransaccion
                .Where(t => t.TipoTransaccionID == id)
                .Select(t => new TipoTransaccionDto
                {
                    TipoTransaccionID = t.TipoTransaccionID,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion
                })
                .FirstOrDefaultAsync();

                if (tipo == null)
                {
                    return NotFound(new { message = $"TipoTransaccion con ID {id} no encontrada." });
                }

                return Ok(tipo);
            }
        }

        // PUT: api/TipoTransacciones/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutTipoTransaccion(int id, UpdateTipoTransaccionDto tipoDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/TipoTransacciones/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(tipoDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar el Tipo de transaccion en la API del Core." });
                }
            }
            else
            {
                var existingTipo = await _context.TiposTransaccion.FindAsync(id);

                if (existingTipo == null)
                {
                    return NotFound(new { message = $"TipoTransaccion con ID {id} no encontrada." });
                }

                existingTipo.Nombre = tipoDto.Nombre;
                existingTipo.Descripcion = tipoDto.Descripcion;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoTransaccionExists(id))
                    {
                        return NotFound(new { message = $"TipoTransaccion con ID {id} no encontrada." });
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { message = "TipoTransaccion actualizada correctamente.", tipo = existingTipo });
            }
        }

        // POST: api/TipoTransacciones
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TipoTransaccion>> PostTipoTransaccion(CreateTipoTransaccionDto tipoDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/TipoTransacciones")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(tipoDto), Encoding.UTF8, "application/json")
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
                    return BadRequest(new { message = "Error al crear el Tipo de transaccion en la API del Core." });
                }
            }
            else
            {
                var tipo = new TipoTransaccion
                {
                    Nombre = tipoDto.Nombre,
                    Descripcion = tipoDto.Descripcion
                };

                _context.TiposTransaccion.Add(tipo);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTipoTransaccion), new { id = tipo.TipoTransaccionID }, tipo);
            }
        }

        // DELETE: api/TipoTransacciones/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTipoTransaccion(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/TipoTransacciones/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar el Tipo de transaccion en la API del Core." });
                }
            }
            else
            {
                var tipo = await _context.TiposTransaccion.FindAsync(id);
                if (tipo == null)
                {
                    return NotFound(new { message = $"TipoTransaccion con ID {id} no encontrada." });
                }

                _context.TiposTransaccion.Remove(tipo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "TipoTransaccion eliminada correctamente.", tipo });
            }
        }

        private bool TipoTransaccionExists(int id)
        {
            return _context.TiposTransaccion.Any(e => e.TipoTransaccionID == id);
        }
    }
}
