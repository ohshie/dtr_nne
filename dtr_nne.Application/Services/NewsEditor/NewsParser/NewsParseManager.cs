using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ScrapableEntities;

[assembly:InternalsVisibleTo("Tests")]
namespace dtr_nne.Application.Services.NewsEditor.NewsParser;

internal class NewsParseManager(ILogger<NewsParseManager> logger,
    IBatchNewsParser batchNewsParser, INewsParser newsParser,
    IArticleMapper mapper) : INewsParseManager
{
    public async Task<ErrorOr<List<NewsArticleDto>>> ExecuteBatchParse(bool fullProcess = true)
    {
        logger.LogInformation("Starting batch parse operation. FullProcess: {FullProcess}", 
            fullProcess);

        var result = await batchNewsParser.ExecuteBatchParse(fullProcess);
        if (result.IsError)
        {
            logger.LogError("Batch parse finished with error: {BatchParseError}", result.FirstError.Description);
            return result.FirstError;
        }
        
        logger.LogInformation("Successfully processed {Count} articles", result.Value.Count);
        var newsArticleDtos = mapper.MassNewsArticleToDto(result.Value);
        return newsArticleDtos;
    }

    public async Task<ErrorOr<NewsArticleDto>> ExecuteParse(Uri articleUri)
    {
        logger.LogInformation("Starting single article processing for URL: {Url}", articleUri.AbsoluteUri);
        var newsArticle = CreateNewsArticleFromUri(articleUri);

        var result = await newsParser.ExecuteParse(newsArticle);
        if (result.IsError)
        {
            logger.LogError("Parse finished with error: {ParseError}", result.FirstError.Description);
            return result.FirstError;
        }

        var processedArticleDto = mapper.NewsArticleToDto(result.Value);
        logger.LogInformation("Successfully processed article with title: {Url}", processedArticleDto.Uri);
        return processedArticleDto;
    }

    private NewsArticle CreateNewsArticleFromUri(Uri newsArticleUri)
    {
        return new NewsArticle
        {
            Website = newsArticleUri
        };
    }
}

public interface INewsParseManager
{
    public Task<ErrorOr<List<NewsArticleDto>>> ExecuteBatchParse(bool fullProcess = true);
    public Task<ErrorOr<NewsArticleDto>> ExecuteParse(Uri articleUri);
}