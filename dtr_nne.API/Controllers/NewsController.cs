using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class NewsController(INewsParser newsParser, INewsRewriter rewriter) : ControllerBase
{
    [HttpPost("RewriteNews", Name = "RewriteNews")]
    [ProducesResponseType<ArticleContentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RewriteNews(ArticleContentDto articleContentDto)
    {
        var editedArticle = await rewriter.Rewrite(articleContentDto);
        
        if (editedArticle.IsError)
        {
            return BadRequest(editedArticle.Errors);
        }

        return Ok(editedArticle.Value);
    }
    
    [HttpPost("ParseNews", Name = "ParseNews")]
    [ProducesResponseType<List<NewsArticleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<NoContent>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ParseNews(bool fullProcess = true)
    {
        var newsArticles = await newsParser.ExecuteBatchParse();
        if (newsArticles.IsError)
        {
            return BadRequest(newsArticles.FirstError);
        }

        if (newsArticles.Value.Count < 1)
        {
            return NoContent();
        }

        return Ok(newsArticles.Value);
    }
    
    [HttpPost("ParseArticle", Name = "ParseArticle")]
    [ProducesResponseType<List<NewsArticleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ParseArticle(BaseNewsArticleDto articleDto)
    {
        var newsArticles = await newsParser.Execute(articleDto);
        if (newsArticles.IsError)
        {
            return BadRequest(newsArticles.FirstError);
        }

        return Ok(newsArticles.Value);
    }
}