using dtr_nne.Application.DTO.Llm;
using dtr_nne.Application.ExternalServices.LlmServices;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("[controller]")]
public class LlmApiController(ILlmApiKeyService service) : ControllerBase
{
    [HttpPost("Add", Name = "Add LlmApiKey")]
    public async Task<ActionResult> Add(LlmApiDto apiKey)
    {
        var success = await service.Add(apiKey);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), apiKey);
    }
    
    [HttpPatch("Patch", Name = "Update LlmapiKey")]
    public async Task<ActionResult> UpdateKey(LlmApiDto apiKey)
    {
        var success = await service.UpdateKey(apiKey);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(apiKey.ApiKey);
    }
}