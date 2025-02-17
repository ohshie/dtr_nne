using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExternalServiceController(IGetExternalService getExternalService, IAddExternalService addExternalService, 
    IUpdateExternalService updateExternalService, 
    IDeleteExternalService deleteExternalService) : ControllerBase
{
    [HttpGet("GetAllByType", Name = "Get External Services by type")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAll(ExternalServiceType type)
    {
        var services = await getExternalService.GetAllByType(type);
        
        if (services.IsError)
        {
            return BadRequest(services.FirstError);
        }
        
        return Ok(services.Value);
    }
    
    [HttpPost("Add", Name = "Add External Service")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Add(ExternalServiceDto service)
    {
        var addedService = await addExternalService.Add(service);

        if (addedService.IsError)
        {
            return BadRequest(addedService.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), addedService.Value);
    }
    
    [HttpPatch("Patch", Name = "Update External Service")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(ExternalServiceDto service)
    {
        var success = await updateExternalService.Update(service);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(service);
    }
    
    [HttpDelete("Delete", Name = "Delete External Service")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(BaseExternalServiceDto service)
    {
        var success = await deleteExternalService.Delete(service);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(service);
    }
}