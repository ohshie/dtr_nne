using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Controllers;
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
        
        Sut = new NewsController(MockNewsRewriter.Object);
    }

    private void BaseSetup()
    {
        MockNewsRewriter
            .Setup(rewriter => rewriter.Rewrite(TestArticleDto).Result)
            .Returns(TestProcessedArticleDto);
    }
}