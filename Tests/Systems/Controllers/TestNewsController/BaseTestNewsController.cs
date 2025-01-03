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

    internal readonly ArticleDto TestArticleDto = new()
    {
        Body = "test"
    };
    
    internal readonly ArticleDto TestProcessedArticleDto = new()
    {
        Body = "rewriten test"
    };
    
    public BaseTestNewsController()
    {
        MockNewsRewriter = new();
        
        BaseSetup();
        
        Sut = new NewsController(new Mock<ILogger<NewsController>>().Object,
            MockNewsRewriter.Object);
    }

    private void BaseSetup()
    {
        MockNewsRewriter
            .Setup(rewriter => rewriter.Rewrite(TestArticleDto).Result)
            .Returns(TestProcessedArticleDto);
    }
}