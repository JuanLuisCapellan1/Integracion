using IntegracionBanco.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IntegracionBanco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly CoreStatusService _coreStatusService;

        public StatusController(CoreStatusService coreStatusService)
        {
            _coreStatusService = coreStatusService;
        }

        [HttpGet("core-status")]
        public async Task<IActionResult> GetCoreStatus()
        {
            bool isCoreActive = await _coreStatusService.IsCoreApiActiveAsync();
            return Ok(new { CoreApiActive = isCoreActive });
        }
    }
}
