using Bogus;
using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Services.NewsEditor.NewsParser;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Controllers;
using NSubstitute;

namespace Tests.Systems.Controllers.TestNewsController;

public class BaseTestNewsController
{
    internal readonly NewsController Sut;

    private static readonly Faker Faker = new();

    internal readonly INewsRewriter MockNewsRewriter = Substitute.For<INewsRewriter>();
    internal readonly INewsParseManager MockNewsParser = Substitute.For<INewsParseManager>();

    internal readonly NewsArticleDto TestArticleDto = new()
    {
        Uri = new Uri(Faker.Internet.Url())
    };
    
    internal readonly ArticleContentDto TestArticleContentDto = new()
    {
        Body = "test"
    };
    
    internal readonly ArticleContentDto TestProcessedArticleContentDto = new()
    {
        Body = "rewriten test"
    };
    
    public BaseTestNewsController()
    {
        BaseSetup();
        
        Sut = new NewsController(MockNewsParser, MockNewsRewriter);
    }

    private void BaseSetup()
    {
        MockNewsRewriter
            .Rewrite(TestArticleContentDto)
            .Returns(TestProcessedArticleContentDto);

        MockNewsParser
            .ExecuteBatchParse()
            .Returns(new List<NewsArticleDto>([new NewsArticleDto()]));
    }
}