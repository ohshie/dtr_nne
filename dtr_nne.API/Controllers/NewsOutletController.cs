using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsOutletController(IGetNewsOutletService getNewsOutletService, 
    IAddNewsOutletService addNewsOutletService, 
    IUpdateNewsOutletService updateNewsOutletService, 
    IDeleteNewsOutletService deleteNewsOutletService) : ControllerBase
{
    [HttpGet("Get", Name = "Get")]
    public async Task<ActionResult> Get()
    {
        var newsOutlets = await getNewsOutletService.GetAllNewsOutlets();

        if (newsOutlets.Count != 0)
        {
            return Ok(newsOutlets);
        }
        
        return NotFound(newsOutlets);
    }

    [HttpPost("Add", Name = "Add Outlet")]
    public async Task<ActionResult> Add(List<NewsOutletDto> newsOutletDtos)
    {
        var addedNewsOutletDtos = await addNewsOutletService.AddNewsOutlets(newsOutletDtos);

        if (addedNewsOutletDtos.Count == 0)
        {
            return UnprocessableEntity();
        }
        
        return CreatedAtAction(nameof(Add), addedNewsOutletDtos);
    }

    [HttpPut("Update", Name = "Update")]
    public async Task<ActionResult> Update(List<NewsOutletDto> newsOutletDtos)
    {
        var updatedNewsOutletDtos = await updateNewsOutletService.UpdateNewsOutlets(newsOutletDtos);

        if (updatedNewsOutletDtos.Count == 0)
        {
            return UnprocessableEntity();
        }
        
        return Ok(updatedNewsOutletDtos);
    }

    [HttpDelete("Delete", Name = "Delete")]
    public async Task<ActionResult> Delete(List<BaseNewsOutletsDto> newsOutletDtos)
    {
        var resultOfDeletion = await deleteNewsOutletService.DeleteNewsOutlets(newsOutletDtos);

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