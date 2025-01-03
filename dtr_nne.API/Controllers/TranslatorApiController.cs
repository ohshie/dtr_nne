using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.ExternalServices.TranslatorServices;
using dtr_nne.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslatorApiController(ITranslatorApiKeyService translatorApiKeyService) : ControllerBase
{
    [HttpPost("Add", Name = "Add TranslatorApiKey")]
    [ProducesResponseType<ExternalService>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Add(ExternalServiceDto apiKey)
    {
        var success = await translatorApiKeyService.Add(apiKey);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), apiKey);
    }

    [HttpPatch("Patch", Name = "UpdateKey")]
    [ProducesResponseType<ExternalService>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateKey(ExternalServiceDto translatorApiDto)
    {
        var success = await translatorApiKeyService.UpdateKey(translatorApiDto);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(translatorApiDto.ApiKey);
    }
}