using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.Services.NewsEditor.NewsParser.NewsSearcher;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class NewsController(INewsSearcher newsSearcher, IExternalServiceProvider provider, INewsRewriter rewriter) : ControllerBase
{
    [HttpPost("RewriteNews", Name = "RewriteNews")]
    [ProducesResponseType<ArticleContentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RewriteNews(ArticleContentDto articleContentDto)
    {
        var editedArticle = await rewriter.Rewrite(articleContentDto);
        
        if (editedArticle.IsError)
        {
            return BadRequest(editedArticle.FirstError);
        }

        return Ok(editedArticle);
    }

    [HttpPost("CompileNews", Name = "CompileNews")]
    [ProducesResponseType<ArticleContentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CompileNews(string? cutOffTime = null)
    {
        var service = provider.Provide(ExternalServiceType.Scraper) as IScrapingService;
        var result = await newsSearcher.CollectNews(service!);

        return Ok();
    }
}