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
        var newsOutlet = await newsOutletService.GetAllNewsOutlets();

        if (newsOutlet.Count != 0)
        {
            return Ok(newsOutlet);
        }
        
        return NotFound(newsOutlet);
    }
}