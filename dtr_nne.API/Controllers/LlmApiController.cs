using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Services.ExternalServices;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlmApiController(IExternalServiceManager serviceManager) : ControllerBase
{
    [HttpPost("Add", Name = "Add LlmApiKey")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Add(ExternalServiceDto service)
    {
        var success = await serviceManager.Add(service);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), service);
    }
    
    [HttpPatch("Patch", Name = "Update LlmapiKey")]
    [ProducesResponseType<ExternalServiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateKey(ExternalServiceDto service)
    {
        var success = await serviceManager.Update(service);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(service.ApiKey);
    }
}