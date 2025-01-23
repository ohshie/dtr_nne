using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.EntityManager;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsOutletController(IGetManagerEntity<NewsOutletDto> getNewsOutletService, 
    IAddManagedEntity<NewsOutletDto> addNewsOutletService, 
    IUpdateManagedEntity<NewsOutletDto> updateNewsOutletService, 
    IDeleteManagedEntity<NewsOutletDto> deleteNewsOutletService) : ControllerBase
{
    [HttpGet("Get", Name = "Get")]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Get()
    {
        var newsOutlets = await getNewsOutletService.GetAll();

        if (newsOutlets.IsError)
        {
            return NotFound(newsOutlets);
        }
        
        return Ok(newsOutlets);
    }

    [HttpPost("Add", Name = "Add Outlet")]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<Error>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Add(List<NewsOutletDto> newsOutletDtos)
    {
        var addedNewsOutletDtos = await addNewsOutletService.Add(newsOutletDtos);

        if (addedNewsOutletDtos.IsError)
        {
            return UnprocessableEntity(addedNewsOutletDtos);
        }
        
        return CreatedAtAction(nameof(Add), addedNewsOutletDtos);
    }

    [HttpPut("Update", Name = "Update")]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(List<NewsOutletDto> newsOutletDtos)
    {
        var resultOfUpdate = await updateNewsOutletService.Update(newsOutletDtos);

        if (resultOfUpdate.IsError)
        {
            return resultOfUpdate.FirstError.Type switch
            {
                ErrorType.Validation => BadRequest(resultOfUpdate.Errors),
                _ => StatusCode(500)
            };
        }
        
        if (resultOfUpdate.Value.Count == 0)
        {
            return Ok();
        }
        
        return StatusCode(206, resultOfUpdate.Value);
    }

    [HttpDelete("Delete", Name = "Delete")]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<NewsOutletDto>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(List<NewsOutletDto> newsOutletDtos)
    {
        var resultOfDeletion = await deleteNewsOutletService.Delete(newsOutletDtos);

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
            return Ok(resultOfDeletion.Value);
        }
        
        return StatusCode(206, resultOfDeletion.Value);
    }
}