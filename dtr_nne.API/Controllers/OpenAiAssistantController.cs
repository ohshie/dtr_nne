using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Services.EntityManager;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OpenAiAssistantController(IGetManagerEntity<OpenAiAssistantDto> getManagedEntityService, 
    IAddManagedEntity<OpenAiAssistantDto> addManagedEntityService, 
    IUpdateManagedEntity<OpenAiAssistantDto> updateManagedEntityService, 
    IDeleteManagedEntity<OpenAiAssistantDto> deleteManagedEntityService) : ControllerBase
{
    [HttpGet("Get", Name = "Get assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Get()
    {
        var newsOutlets = await getManagedEntityService.GetAll();

        if (newsOutlets.IsError)
        {
            return NotFound(newsOutlets);
        }
        
        return Ok(newsOutlets.Value);
    }

    [HttpPost("Add", Name = "Add assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<Error>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Add(List<OpenAiAssistantDto> managedEntityDtos)
    {
        var addedNewsOutletDtos = await addManagedEntityService.Add(managedEntityDtos);

        if (addedNewsOutletDtos.IsError)
        {
            return UnprocessableEntity(addedNewsOutletDtos);
        }
        
        return CreatedAtAction(nameof(Add), addedNewsOutletDtos.Value);
    }

    [HttpPut("Update", Name = "Update assistants")]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<OpenAiAssistantDto>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(List<OpenAiAssistantDto> managedEntityDtos)
    {
        var resultOfUpdate = await updateManagedEntityService.Update(managedEntityDtos);

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
    public async Task<ActionResult> Delete(List<OpenAiAssistantDto> managedEntityDtos)
    {
        var resultOfDeletion = await deleteManagedEntityService.Delete(managedEntityDtos);

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