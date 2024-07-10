using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core_Banco.Data;
using Core_Banco.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegracionBanco.Services;
using System.Configuration;
using System.Text;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoCuentaController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;

        public TipoCuentaController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/TipoCuenta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoCuenta>>> GetTipoCuentas()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/TipoCuenta");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                return await _context.TipoCuentas.ToListAsync();
            }
        }

        // GET: api/TipoCuenta/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoCuenta>> GetTipoCuenta(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/TipoCuenta{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var tipoCuenta = await _context.TipoCuentas.FindAsync(id);

                if (tipoCuenta == null)
                {
                    return NotFound();
                }

                return tipoCuenta;
            }
        }

        // POST: api/TipoCuenta
        [HttpPost]
        public async Task<ActionResult<TipoCuenta>> PostTipoCuenta(TipoCuenta.CreateTipoCuentaDto createTipoCuentaDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/TipoCuenta")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(createTipoCuentaDto), Encoding.UTF8, "application/json")
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
                    return BadRequest(new { message = "Error al crear el cliente en la API del Core." });
                }
            }
            else
            {
                var tipoCuenta = new TipoCuenta
                {
                    Nombre = createTipoCuentaDto.Nombre
                };

                _context.TipoCuentas.Add(tipoCuenta);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTipoCuenta", new { id = tipoCuenta.TipoCuentaID }, tipoCuenta);
            }
        }

        // PUT: api/TipoCuenta/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoCuenta(int id, TipoCuenta.UpdateTipoCuentaDto updateTipoCuentaDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/TipoCuenta/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(updateTipoCuentaDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar el cliente en la API del Core." });
                }
            }
            else
            {
                var tipoCuenta = await _context.TipoCuentas.FindAsync(id);
                if (tipoCuenta == null)
                {
                    return NotFound();
                }

                tipoCuenta.Nombre = updateTipoCuentaDto.Nombre;

                _context.Entry(tipoCuenta).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoCuentaExists(id))
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

        // DELETE: api/TipoCuenta/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoCuenta(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/TipoCuenta/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar el cliente en la API del Core." });
                }
            }
            else
            {
                var tipoCuenta = await _context.TipoCuentas.FindAsync(id);
                if (tipoCuenta == null)
                {
                    return NotFound();
                }

                _context.TipoCuentas.Remove(tipoCuenta);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }

        private bool TipoCuentaExists(int id)
        {
            return _context.TipoCuentas.Any(e => e.TipoCuentaID == id);
        }
    }
}
