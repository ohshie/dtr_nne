using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Services.NewsArticleManager;
using Microsoft.AspNetCore.Mvc;

namespace dtr_nne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsArticleController(IGetNewsArticles getNewsArticles,
    IDeleteNewsArticle deleteNewsArticle) : ControllerBase
{
    [HttpGet("Get", Name = "Get News Articles")]
    [ProducesResponseType<NewsArticleDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult> Get()
    {
        var newsOutlets = await getNewsArticles.GetAll();

        return newsOutlets.IsError ? Ok(newsOutlets.Errors) : Ok(newsOutlets.Value);
    }
    
    [HttpDelete("Delete", Name = "Delete News Articles")]
    [ProducesResponseType<NewsArticleDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult> Delete(List<BaseNewsArticleDto> newsArticleDtos)
    {
        var deletedArticles = await deleteNewsArticle.Delete(newsArticleDtos);
        
        return deletedArticles.IsError ? Ok(deletedArticles.Errors) : Ok(deletedArticles.Value);
    }
}