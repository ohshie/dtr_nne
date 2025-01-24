using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Services.ExternalServices;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExternalServiceController(IAddExternalService addExternalService, 
    IUpdateExternalService updateExternalService) : ControllerBase
{
    [HttpPost("Add", Name = "Add External Service")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Add(ExternalServiceDto service)
    {
        var success = await addExternalService.Add(service);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), service);
    }
    
    [HttpPatch("Patch", Name = "Update External Service")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateKey(ExternalServiceDto service)
    {
        var success = await updateExternalService.Update(service);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(service.ApiKey);
    }
}