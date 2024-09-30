using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.TranslatorServices;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("[controller]")]
public class TranslatorApiController(ITranslatorApiKeyService translatorApiKeyService) : ControllerBase
{
    [HttpPost("Add", Name = "Add TranslatorApiKey")]
    public async Task<ActionResult> Add(TranslatorApiDto apiKey)
    {
        var success = await translatorApiKeyService.Add(apiKey);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), apiKey);
    }

    [HttpPatch("Patch", Name = "UpdateKey")]
    public async Task<ActionResult> UpdateKey(TranslatorApiDto translatorApiDto)
    {
        var success = await translatorApiKeyService.UpdateKey(translatorApiDto);
        
        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return Ok(translatorApiDto.ApiKey);
    }
}