using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.NewsOutletServices;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
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

    [HttpPost("Add", Name = "Add Outlet")]
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

    [HttpDelete("Delete", Name = "Delete")]
    public async Task<ActionResult> Delete(List<DeleteNewsOutletsDto> newsOutletDtos)
    {
        var resultOfDeletion = await newsOutletService.DeleteNewsOutlets(newsOutletDtos);

        if (resultOfDeletion.IsError)
        {
            return resultOfDeletion.FirstError.Type switch
            {
                ErrorType.NotFound => BadRequest(resultOfDeletion.Errors),
                _ => StatusCode(500)
            };
        }
        
        if (resultOfDeletion.Value.Count == 0)
        {
            Ok(resultOfDeletion.Value);
        }
        
        return Ok(resultOfDeletion.Value);
    }
}