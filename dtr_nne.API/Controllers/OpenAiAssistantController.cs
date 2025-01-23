using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Services.EntityManager;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAiAssistantController(IGetManagerEntity<OpenAiAssistantDto> getNewsOutletService, 
    IAddManagedEntity<OpenAiAssistantDto> addNewsOutletService, 
    IUpdateManagedEntity<OpenAiAssistantDto> updateNewsOutletService, 
    IDeleteManagedEntity<OpenAiAssistantDto> deleteNewsOutletService) : ControllerBase
{
    [HttpGet("Get", Name = "Get assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status200OK)]
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

    [HttpPost("Add", Name = "Add assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<Error>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Add(List<OpenAiAssistantDto> newsOutletDtos)
    {
        var addedNewsOutletDtos = await addNewsOutletService.Add(newsOutletDtos);

        if (addedNewsOutletDtos.IsError)
        {
            return UnprocessableEntity(addedNewsOutletDtos);
        }
        
        return CreatedAtAction(nameof(Add), addedNewsOutletDtos);
    }

    [HttpPut("Update", Name = "Update assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(List<OpenAiAssistantDto> newsOutletDtos)
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

    [HttpDelete("Delete", Name = "Delete assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(List<OpenAiAssistantDto> newsOutletDtos)
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