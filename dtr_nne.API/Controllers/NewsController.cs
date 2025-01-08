using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class NewsController(INewsRewriter rewriter) : ControllerBase
{
    [HttpPost("RewriteNews", Name = "RewriteNews")]
    [ProducesResponseType<ArticleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RewriteNews(ArticleDto articleDto)
    {
        var editedArticle = await rewriter.Rewrite(articleDto);
        
        if (editedArticle.IsError)
        {
            return BadRequest(editedArticle.FirstError);
        }

        return Ok(editedArticle);
    }
}