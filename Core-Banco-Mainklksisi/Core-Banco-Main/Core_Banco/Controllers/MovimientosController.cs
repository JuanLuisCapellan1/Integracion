using Core_Banco.Data;
using Core_Banco.Models;
using IntegracionBanco.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todo el controlador
    public class MovimientosController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly ILogger<MovimientosController> _logger;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;
        public MovimientosController(Core_BancoContext context, ILogger<MovimientosController> logger, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // POST: api/Movimientos
        [HttpPost]
        [Authorize(Roles = "Admin,Mantenimiento")]
        public async Task<ActionResult<Movimiento>> PostMovimiento(CreateMovimientoDto movimientoDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Movimientos")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(movimientoDto), Encoding.UTF8, "application/json")
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
                    return BadRequest(new { message = "Error al crear el movimiento en la API del Core." });
                }
            }
            else
            {
                var userPerfilId = GetUserPerfilId();

                if (userPerfilId == 3)
                {
                    return Forbid("Usuarios con PerfilID 3 no pueden realizar esta acción.");
                }

                var cuenta = await _context.Cuentas.FindAsync(movimientoDto.CuentaID);
                if (cuenta == null)
                {
                    _logger.LogWarning($"Cuenta con ID {movimientoDto.CuentaID} no encontrada.");
                    return NotFound(new { message = $"Cuenta con ID {movimientoDto.CuentaID} no encontrada." });
                }

                var tipoTransaccion = await _context.TiposTransaccion.FindAsync(movimientoDto.TipoTransaccionID);
                if (tipoTransaccion == null)
                {
                    _logger.LogWarning($"Tipo de Transacción con ID {movimientoDto.TipoTransaccionID} no encontrado.");
                    return NotFound(new { message = $"Tipo de Transacción con ID {movimientoDto.TipoTransaccionID} no encontrado." });
                }

                var movimiento = new Movimiento
                {
                    CuentaID = movimientoDto.CuentaID,
                    TipoTransaccionID = movimientoDto.TipoTransaccionID,
                    Monto = movimientoDto.Monto,
                    FechaTransaccion = movimientoDto.FechaTransaccion
                };

                _context.Movimientos.Add(movimiento);

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Movimiento para la cuenta ID {movimiento.CuentaID} registrado correctamente.");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al registrar el movimiento.");
                    return StatusCode(500, "Ocurrió un error al registrar el movimiento.");
                }

                return CreatedAtAction(nameof(GetMovimientosPorCuenta), new { cuentaId = movimiento.CuentaID }, movimiento);
            }
        }

        // GET: api/Movimientos/Cuenta/5
        [HttpGet("Cuenta/{cuentaId}")]
        [Authorize(Roles = "Admin,Mantenimiento,Usuario")]
        public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetMovimientosPorCuenta(int cuentaId)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Movimientos/Cuenta{cuentaId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var movimientos = await _context.Movimientos
                .Where(m => m.CuentaID == cuentaId)
                .Include(m => m.TipoTransaccion) // Incluir TipoTransaccion si es necesario
                .Select(m => new MovimientoDto
                {
                    Id = m.TipoTransaccionID,
                    MovimientoID = m.MovimientoID,
                    CuentaID = m.CuentaID,
                    TipoTransaccionID = m.TipoTransaccionID,
                    Monto = m.Monto,
                    FechaTransaccion = m.FechaTransaccion,
                    TipoTransaccion = new TipoTransaccionDto
                    {
                        TipoTransaccionID = m.TipoTransaccion.TipoTransaccionID,
                        Nombre = m.TipoTransaccion.Nombre,
                        Descripcion = m.TipoTransaccion.Descripcion
                    }
                })
                .ToListAsync();

                if (movimientos == null || movimientos.Count == 0)
                {
                    _logger.LogWarning($"No se encontraron movimientos para la cuenta con ID {cuentaId}.");
                    return NotFound(new { message = $"No se encontraron movimientos para la cuenta con ID {cuentaId}." });
                }

                return Ok(movimientos);
            }
        }

        private int GetUserPerfilId()
        {
            var userName = User.Identity.Name;
            var user = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == userName);
            return user?.PerfilID ?? 0;
        }
    }
}
