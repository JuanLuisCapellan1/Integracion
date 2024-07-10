using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core_Banco.Data;
using Core_Banco.Models;
using IntegracionBanco.Services;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Text;
using Humanizer.Configuration;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiariosController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;

        public BeneficiariosController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/Beneficiarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Beneficiario>>> GetBeneficiarios()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Beneficiarios");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                return await _context.Beneficiarios.ToListAsync();
            }
        }

        // GET: api/Beneficiarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Beneficiario>> GetBeneficiario(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Beneficiarios/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id);

                if (beneficiario == null)
                {
                    return NotFound();
                }

                return beneficiario;
            }
           
        }

        // POST: api/Beneficiarios
        [HttpPost]
        public async Task<ActionResult<Beneficiario>> PostBeneficiario(Beneficiario.CreateDto beneficiarioDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Beneficiarios")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(beneficiarioDto), Encoding.UTF8, "application/json")
                };

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    var coreContent = await coreResponse.Content.ReadAsStringAsync();
                    return Content(coreContent, coreResponse.Content.Headers.ContentType?.ToString());
                }
                else
                {
                    return BadRequest(new { message = "Error al crear el beneficiario en la API del Core." });
                }
            }
            else
            {
                var beneficiario = new Beneficiario
                {
                    Nombre = beneficiarioDto.Nombre,
                    CuentaID = beneficiarioDto.CuentaID,
                    UsuarioID = beneficiarioDto.UsuarioID
                };

                _context.Beneficiarios.Add(beneficiario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBeneficiario), new { id = beneficiario.BeneficiarioID }, beneficiario);
            }
        }

        // PUT: api/Beneficiarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBeneficiario(int id, Beneficiario.UpdateDto beneficiarioDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/Beneficiarios/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(beneficiarioDto), Encoding.UTF8, "application/json")
                };

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar el beneficiario en la API del Core." });
                }
            }
            else
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id);

                if (beneficiario == null)
                {
                    return NotFound();
                }

                beneficiario.Nombre = beneficiarioDto.Nombre;
                beneficiario.CuentaID = beneficiarioDto.CuentaID;
                beneficiario.UsuarioID = beneficiarioDto.UsuarioID;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) when (!_context.Beneficiarios.Any(e => e.BeneficiarioID == id))
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        // DELETE: api/Beneficiarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBeneficiario(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/Beneficiarios/{id}");

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar el beneficiario en la API del Core." });
                }
            }
            else
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id);
                if (beneficiario == null)
                {
                    return NotFound();
                }

                _context.Beneficiarios.Remove(beneficiario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }
}
