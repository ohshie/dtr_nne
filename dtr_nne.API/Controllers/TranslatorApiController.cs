using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.TranslatorServices;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("[controller]")]
public class TranslatorApiController(ITranslatorApiKeyService translatorApiKeyService) : ControllerBase
{
    [HttpPost("Add", Name = "Add")]
    public async Task<ActionResult> Add(TranslatorApiDto apiKey)
    {
        var success = await translatorApiKeyService.Add(apiKey);

        if (success.IsError)
        {
            return BadRequest(success.FirstError);
        }
        
        return CreatedAtAction(nameof(Add), apiKey);
    }
}