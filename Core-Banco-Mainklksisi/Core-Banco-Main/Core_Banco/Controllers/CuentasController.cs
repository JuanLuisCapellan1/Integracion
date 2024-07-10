﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core_Banco.Data;
using Core_Banco.Models;
using IntegracionBanco.Services;
using System.Text;

namespace Core_Banco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todo el controlador
    public class CuentasController : ControllerBase
    {
        private readonly Core_BancoContext _context;
        private readonly CoreStatusService _coreStatusService;
        private readonly CoreApiService _coreApiService;
        private readonly IConfiguration _configuration;

        public CuentasController(Core_BancoContext context, CoreStatusService coreStatusService, CoreApiService coreApiService, IConfiguration configuration)
        {
            _context = context;
            _coreStatusService = coreStatusService;
            _coreApiService = coreApiService;
            _configuration = configuration;
        }

        // GET: api/Cuentas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaDto>>> GetCuentas()
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Cuentas");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                return await _context.Cuentas
               .Include(c => c.Cliente)
               .Select(c => new CuentaDto
               {
                   CuentaId = c.CuentaID,
                   ClienteId = c.ClienteID,
                   TipoCuentaID = c.TipoCuentaID,
                   Balance = c.Balance,
                   FechaCreacion = c.FechaCreacion,
                   ClienteNombre = c.Cliente.Nombre,
                   ClienteApellido = c.Cliente.Apellido,
                   ClienteDocumentoIdentidad = c.Cliente.DocumentoIdentidad
               })
               .ToListAsync();
            }
        }

        // GET: api/Cuentas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaDto>> GetCuenta(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Cuentas/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var cuenta = await _context.Cuentas
               .Include(c => c.Cliente)
               .Select(c => new CuentaDto
               {
                   CuentaId = c.CuentaID,
                   ClienteId = c.ClienteID,
                   TipoCuentaID = c.TipoCuentaID,
                   Balance = c.Balance,
                   FechaCreacion = c.FechaCreacion,
                   ClienteNombre = c.Cliente.Nombre,
                   ClienteApellido = c.Cliente.Apellido,
                   ClienteDocumentoIdentidad = c.Cliente.DocumentoIdentidad
               })
               .FirstOrDefaultAsync(c => c.CuentaId == id);

                if (cuenta == null)
                {
                    return NotFound(new { message = $"Cuenta con ID {id} no encontrada." });
                }

                return cuenta;
            }
        }

        // GET: api/Cuentas/Cliente/5
        [HttpGet("Cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<CuentaDto>>> GetCuentasByClienteId(int clienteId)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                // Obtener el token del contexto de la solicitud actual
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{coreApiUrl}/api/Cuentas/Cliente/{clienteId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);

                var content = await coreResponse.Content.ReadAsStringAsync();
                return Content(content, coreResponse.Content.Headers.ContentType?.ToString());
            }
            else
            {
                var cuentas = await _context.Cuentas
               .Include(c => c.Cliente)
               .Where(c => c.ClienteID == clienteId)
               .Select(c => new CuentaDto
               {
                   CuentaId = c.CuentaID,
                   ClienteId = c.ClienteID,
                   TipoCuentaID = c.TipoCuentaID,
                   Balance = c.Balance,
                   FechaCreacion = c.FechaCreacion,
                   ClienteNombre = c.Cliente.Nombre,
                   ClienteApellido = c.Cliente.Apellido,
                   ClienteDocumentoIdentidad = c.Cliente.DocumentoIdentidad
               })
               .ToListAsync();

                if (cuentas == null || cuentas.Count == 0)
                {
                    return NotFound(new { message = $"No se encontraron cuentas para el cliente con ID {clienteId}." });
                }

                return cuentas;
            }
        }

        // PUT: api/Cuentas/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Mantenimiento")]
        public async Task<IActionResult> PutCuenta(int id, UpdateCuentaDto cuentaDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Put, $"{coreApiUrl}/api/Cuentas/{id}")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(cuentaDto), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al actualizar la cuenta en la API del Core." });
                }
            }
            else
            {
                var userPerfilId = GetUserPerfilId();

                if (userPerfilId == 2)
                {
                    return Forbid("Usuarios con PerfilID 2 no pueden realizar esta acción.");
                }

                var existingCuenta = await _context.Cuentas.FindAsync(id);

                if (existingCuenta == null)
                {
                    return NotFound(new { message = $"Cuenta con ID {id} no encontrada." });
                }

                existingCuenta.Balance = cuentaDto.Balance;
                existingCuenta.TipoCuentaID = cuentaDto.TipoCuentaID;
                existingCuenta.FechaCreacion = cuentaDto.FechaCreacion;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaExists(id))
                    {
                        return NotFound(new { message = $"Cuenta con ID {id} no encontrada." });
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { message = "Cuenta actualizada correctamente.", cuenta = existingCuenta });
            }
        }

        // POST: api/Cuentas
        [HttpPost]
        [Authorize(Roles = "Admin,Mantenimiento")]
        public async Task<ActionResult<CuentaDto>> PostCuenta(CreateCuentaDto cuentaDto)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{coreApiUrl}/api/Cuentas")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(cuentaDto), Encoding.UTF8, "application/json")
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
                    return BadRequest(new { message = "Error al crear la cuenta en la API del Core." });
                }
            }
            else
            {
                var userPerfilId = GetUserPerfilId();

                if (userPerfilId == 2)
                {
                    return Forbid("Usuarios con PerfilID 2 no pueden realizar esta acción.");
                }

                // Crear una nueva cuenta
                var cuenta = new Cuenta
                {
                    ClienteID = cuentaDto.ClienteID,
                    TipoCuentaID = cuentaDto.TipoCuentaID,
                    Balance = 0,
                    FechaCreacion = cuentaDto.FechaCreacion
                };

                _context.Cuentas.Add(cuenta);
                await _context.SaveChangesAsync();

                // Obtener la información del cliente
                var cliente = await _context.Clientes.FindAsync(cuentaDto.ClienteID);
                if (cliente == null)
                {
                    return NotFound(new { message = $"Cliente con ID {cuentaDto.ClienteID} no encontrado." });
                }

                // Crear y devolver el DTO que incluye la información del cliente
                var cuentaDtoResult = new CuentaDto
                {
                    CuentaId = cuenta.CuentaID,
                    ClienteId = cuenta.ClienteID,
                    TipoCuentaID = cuenta.TipoCuentaID,
                    Balance = cuenta.Balance,
                    FechaCreacion = cuenta.FechaCreacion,
                    ClienteNombre = cliente.Nombre,
                    ClienteApellido = cliente.Apellido,
                    ClienteDocumentoIdentidad = cliente.DocumentoIdentidad
                };

                return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.CuentaID }, cuentaDtoResult);
            }
        }

        // DELETE: api/Cuentas/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCuenta(int id)
        {
            if (await _coreStatusService.IsCoreApiActiveAsync())
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // Crear una solicitud HTTP para la API del Core
                string coreApiUrl = _configuration.GetValue<string>("CoreApiUrl");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{coreApiUrl}/api/Cuentas/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var coreResponse = await _coreApiService.ForwardRequestToCoreApiAsync(request);
                if (coreResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "Error al eliminar la cuenta en la API del Core." });
                }
            }
            else
            {
                var userPerfilId = GetUserPerfilId();

                if (userPerfilId == 2 || userPerfilId == 3)
                {
                    return Forbid("Usuarios con PerfilID 2 y 3 no pueden eliminar cuentas.");
                }

                var cuenta = await _context.Cuentas.FindAsync(id);
                if (cuenta == null)
                {
                    return NotFound(new { message = $"Cuenta con ID {id} no encontrada." });
                }

                _context.Cuentas.Remove(cuenta);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cuenta eliminada correctamente.", cuenta });
            }
        }

        private bool CuentaExists(int id)
        {
            return _context.Cuentas.Any(e => e.CuentaID == id);
        }

        private int GetUserPerfilId()
        {
            var userName = User.Identity.Name;
            var user = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == userName);
            return user?.PerfilID ?? 0;
        }
    }
}
