using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;

namespace dtr_nne.Application.Services.NewsEditor.NewsRewriter;

public class NewsRewriter(ILogger<NewsRewriter> logger, IArticleMapper mapper,
    IExternalServiceProvider serviceProvider) : INewsRewriter
{
    public async Task<ErrorOr<ArticleContentDto>> Rewrite(ArticleContentDto articleContentDto)
    {
        if (await RequestService() is not { } activeService)
        {
            return Errors.ExternalServiceProvider.Service.NoActiveServiceFound;
        }

        var mappedArticle = mapper.DtoToArticleContent(articleContentDto);
        
        var processedArticle = await activeService.ProcessArticleAsync(mappedArticle);
        if (processedArticle.IsError)
        {
            return processedArticle.FirstError;
        }

        var processedArticleDto = mapper.ArticleContentToDto(processedArticle.Value);

        return processedArticleDto;
    }

    internal async Task<ILlmService?> RequestService()
    {
        try
        {
            var service = await serviceProvider.Provide(ExternalServiceType.Llm) as ILlmService;
            return service;
        }
        catch (Exception e)
        {
            logger.LogError(e,"Something went wrong when attempting to fetch currently active existing LLM Serivce: {ErrorStack}, {ErrorMessage}", e.Message, e.StackTrace);
            return null;
        }
    }
}