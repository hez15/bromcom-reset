
using Microsoft.AspNetCore.Mvc;

namespace BromcomReset.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        [HttpGet("healthz")]
        public IActionResult Health() => Ok(new { status = "ok" });
    }
}
