using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecurePortal.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Authenticated Content");
    }
}
