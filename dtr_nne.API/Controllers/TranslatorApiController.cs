using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.ExternalServices.TranslatorServices;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("[controller]")]
public class TranslatorApiController(ITranslatorApiKeyService translatorApiKeyService) : ControllerBase
{
    [HttpPost("Add", Name = "Add TranslatorApiKey")]
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