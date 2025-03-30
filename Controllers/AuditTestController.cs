using AuditLoggingWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuditLoggingWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditTestController : ControllerBase
    {
        [HttpPost("sensitive")]
        public IActionResult PostSensitiveData([FromBody] SensitiveDataModel data)
        {
            return Ok(new { Message = "Data processed", ReceivedData = data });
        }

        [HttpGet("error")]
        public IActionResult GenerateError()
        {
            //throw new InvalidOperationException("This is a test error for audit logging");
            return Ok(new { Message = "This is a test error for audit logging", Timestamp = DateTime.UtcNow });
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            return Ok(new { Message = "Successful request", Timestamp = DateTime.UtcNow });
        }
    }
}
