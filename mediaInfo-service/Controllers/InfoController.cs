using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace _MediaInfoService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        [HttpGet("~/alive")]
        [HttpGet("~/now")]
        public IActionResult GetAlive()
        {
            return Ok(new
                {
                    UtcNow = DateTime.UtcNow.ToString("o")
                });              
        }

        [Produces("application/json")]
        [HttpGet("~/version")]
        public IActionResult GetVersion()
        {
            var assembly = typeof(Program).Assembly;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            return Ok(new
                {
                    Product = product,
                    Version = version,
                });
        }

        [Produces("application/json")]
        [HttpGet("~/ip")]
        public IActionResult Ip()
        {
            var headerSet = new HashSet<string> { "x-forwarded-for", "cf-connecting-ip", "client-ip" };
            var headers = HttpContext.Request?.Headers
                .Where(h => headerSet.Contains(h.Key.ToLower()))
                .ToDictionary(h => h.Key);
            return Ok(new
                {
                    Ip = HttpContext.Connection?.RemoteIpAddress?.ToString(),
                    Headers = headers,
                });
        }
    }
}
