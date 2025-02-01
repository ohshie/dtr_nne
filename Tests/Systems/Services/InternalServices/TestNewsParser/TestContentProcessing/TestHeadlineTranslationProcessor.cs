using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsEditor.NewsParser.ContentProcessing.HeadlineTranslateProcessor;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Fixtures.NewsArticleFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsParser.TestContentProcessing;

public class TestHeadlineTranslationProcessor
{
    public TestHeadlineTranslationProcessor()
    {
        _translatorService = Substitute.For<ITranslatorService>();

        _sut = new HeadlineTranslationProcessor(Substitute.For<ILogger<HeadlineTranslationProcessor>>());
    }

    private readonly HeadlineTranslationProcessor _sut;
    private readonly ITranslatorService _translatorService;
    private readonly List<NewsArticle> _testNewsArticles = NewsArticleFixtureBase.Articles[1].Take(2).ToList();

    [Fact]
    public async Task Translate_WhenSuccess_ReturnsUpdatedArticles()
    {
        // Arrange
        _testNewsArticles[0].ArticleContent!.Headline = new Headline { OriginalHeadline = "Original Headline 1" };
        _testNewsArticles[1].ArticleContent!.Headline = new Headline { OriginalHeadline = "Original Headline 2" };

        var translatedHeadlines = new List<Headline>
        {
            new() { OriginalHeadline = "Original Headline 1", TranslatedHeadline = "Translated Headline 1" },
            new() { OriginalHeadline = "Original Headline 2", TranslatedHeadline = "Translated Headline 2" }
        };

        _translatorService.Translate(Arg.Any<List<Headline>>())
            .Returns(translatedHeadlines);

        // Act
        var result = await _sut.Translate(_translatorService, _testNewsArticles);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value[0].ArticleContent!.Headline.TranslatedHeadline.Should().Be("Translated Headline 1");
        result.Value[1].ArticleContent!.Headline.TranslatedHeadline.Should().Be("Translated Headline 2");
    }
    
    [Fact]
    public async Task Translate_WhenTranslationFails_ReturnsError()
    {
        // Arrange
        var expectedError = Errors.Translator.Api.BadApiKey;
        _translatorService.Translate(Arg.Any<List<Headline>>())
            .Returns(expectedError);

        // Act
        var result = await _sut.Translate(_translatorService, _testNewsArticles);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }
}