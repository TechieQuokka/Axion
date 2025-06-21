using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API is working!",
                timestamp = DateTime.Now,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("debug")]
        [AllowAnonymous]
        public IActionResult Debug()
        {
            return Ok(new
            {
                message = "Debug endpoint working",
                serverTime = DateTime.Now,
                requestHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }
    }
}