using DeepL;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Infrastructure.ExternalServices.TranslatorServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.ExternalServices;

public class TestDeeplTranslator
{
    public TestDeeplTranslator()
    {
        var faker = new Bogus.Faker();
        
        _testExternalService = new ExternalService
        {
            ServiceName = Enum.GetName(typeof(TranslatorServiceType), TranslatorServiceType.Deepl)!,
            InUse = true,
            ApiKey = faker.Lorem.Slug(),
            Type = ExternalServiceType.Translator
        };
        
        _testHeadlines = new List<Headline>(
            [
                new Headline
                {
                    OriginalHeadline = faker.Lorem.Slug(1)
                }
            ]);
        
        _sut = new Mock<DeeplTranslator>(new Mock<ILogger<DeeplTranslator>>().Object, _testExternalService) {CallBase = true};

        _sut
            .Setup(translator => 
                translator.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Returns("Translated");
    }

    private readonly Mock<DeeplTranslator> _sut;
    private readonly List<Headline> _testHeadlines;
    private readonly ExternalService _testExternalService;

    [Fact]
    public async Task Translate_WhenNoHeadlines_ShouldReturnError()
    {
        // Assemble
        _testHeadlines.Clear();

        // Act
        var result = await _sut.Object.Translate(_testHeadlines);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Translator.Service.NoHeadlineProvided);
    }

    [Fact]
    public async Task Translate_WhenHeadlinesProvided_ShouldCallTranslateHeadlines()
    {
        // Assemble

        // Act
        await _sut.Object.Translate(_testHeadlines);

        // Assert 
        _sut.Verify(x => x.TranslateHeadlines(
                _testHeadlines, 
                _testExternalService.ApiKey), 
            Times.Once);
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenEmptyOriginalHeadline_ShouldReturnEmptyTranslation()
    {
        // Assemble
        _testHeadlines[0].OriginalHeadline = "";

        // Act
        var result = await _sut.Object.TranslateHeadlines(_testHeadlines, _testExternalService.ApiKey);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.First().TranslatedHeadline.Should().BeEmpty();
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenBadApiKey_ShouldReturnError()
    {
        // Assemble
        _sut.Reset();
        
        // Act
        var result = await _sut.Object.TranslateHeadlines(_testHeadlines, _testExternalService.ApiKey);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].TranslatedHeadline.Should().Be("While translating service produced: Authorization failure, check AuthKey");
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenQuotaExceeded_ShouldReturnError()
    {
        // Assemble
        _sut.Setup(x => x.PerformRequest(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new QuotaExceededException("Quota exceeded"));

        // Act
        var result = await _sut.Object.TranslateHeadlines(_testHeadlines, _testExternalService.ApiKey);

        // Assert
        result.Value.Any(headline => headline.TranslatedHeadline == Errors.Translator.Api.QuotaExceeded.Description).Should().BeTrue();
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenSuccess_ShouldReturnTranslatedHeadlines()
    {
        // Assemble
        
        // Act
        var result = await _sut.Object.TranslateHeadlines(_testHeadlines, _testExternalService.ApiKey);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.All(h => h.TranslatedHeadline == "Translated").Should().BeTrue();
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenMultipleHeadlines_ShouldUseSemaphore()
    {
        // Assemble
        var headlines = Enumerable.Range(0, 10)
            .Select(i => new Headline { OriginalHeadline = $"Test{i}" })
            .ToList();

        var processingCount = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        _sut.Setup(x => x.PerformRequest(It.IsAny<string>(), _testExternalService.ApiKey).Result)
            .Returns((string _, string _) =>
            {
                lock (lockObj)
                {
                    processingCount++;
                    maxConcurrent = Math.Max(maxConcurrent, processingCount);
                }
                Thread.Sleep(100);
                lock (lockObj)
                {
                    processingCount--;
                }
                return "Translated";
            });

        // Act
        var result = await _sut.Object.TranslateHeadlines(headlines, _testExternalService.ApiKey);

        // Assert
        result.IsError.Should().BeFalse();
        maxConcurrent.Should().BeLessThanOrEqualTo(5);
    }
}