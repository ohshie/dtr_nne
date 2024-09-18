using dtr_nne.Application.DTO;
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
        var newsOutlets = await newsOutletService.GetAllNewsOutlets();

        if (newsOutlets.Count != 0)
        {
            return Ok(newsOutlets);
        }
        
        return NotFound(newsOutlets);
    }

    [HttpGet("Add", Name = "Add")]
    public async Task<ActionResult> Add(List<NewsOutletDto> newsOutletDtos)
    {
        var addedNewsOutletDtos = await newsOutletService.AddNewsOutlets(newsOutletDtos);

        if (addedNewsOutletDtos.Count == 0)
        {
            return UnprocessableEntity();
        }
        
        return CreatedAtAction(nameof(Add), addedNewsOutletDtos);
    }

    [HttpPut("Update", Name = "Update")]
    public async Task<ActionResult> Update(List<NewsOutletDto> newsOutletDtos)
    {
        var updatedNewsOutletDtos = await newsOutletService.UpdateNewsOutlets(newsOutletDtos);

        if (updatedNewsOutletDtos.Count == 0)
        {
            return UnprocessableEntity();
        }
        
        return Ok(updatedNewsOutletDtos);
    }
}