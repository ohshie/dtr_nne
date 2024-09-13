using dtr_nne.Application.NewsOutletServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class NewsOutletController(INewsOutletService newsOutletService) : ControllerBase
{
    [HttpGet("Get", Name = "Get")]
    public async Task<ActionResult> Get()
    {
        return Ok("All good");
    }
}