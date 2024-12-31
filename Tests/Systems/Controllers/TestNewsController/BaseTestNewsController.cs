using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.NewsEditor;
using dtr_nne.Controllers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Controllers.TestNewsController;

public class BaseTestNewsController
{
    internal readonly NewsController Sut;
    internal readonly Mock<INewsRewriter> MockNewsRewriter;

    internal readonly ArticleDto _testArticleDto = new()
    {
        Body = "test"
    };
    
    internal readonly ArticleDto _testProcessedArticleDto = new()
    {
        Body = "rewriten test"
    };
    
    public BaseTestNewsController()
    {
        MockNewsRewriter = new();
        
        Sut = new NewsController(new Mock<ILogger<NewsController>>().Object,
            MockNewsRewriter.Object);
    }

    private void BaseSetup()
    {
        MockNewsRewriter
            .Setup(rewriter => rewriter.Rewrite(_testArticleDto).Result)
            .Returns(_testProcessedArticleDto);
    }
}