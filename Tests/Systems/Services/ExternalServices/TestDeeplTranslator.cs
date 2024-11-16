using DeepL;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Infrastructure.ExternalServices;
using dtr_nne.Infrastructure.ExternalServices.TranslatorServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.ExternalServices;

public class TestDeeplTranslator
{
    public TestDeeplTranslator()
    {
        _mockHeadlines = new();
        _mockTranslatorApi = new();
        _mockRepository = new();
        
        _mockHeadlines.Object.Add(new Headline{OriginalHeadline = Faker.Lorem.GetFirstWord(), TranslatedHeadline = Faker.Name.First()});
        _mockTranslatorApi.Object.ApiKey = Faker.Lorem.GetFirstWord();
        
        _mockRepository.Setup(repository => repository.Get(1).Result).Returns(_mockTranslatorApi.Object);
        
        _sut = new(_mockRepository.Object, new Mock<ILogger<DeeplTranslator>>().Object){CallBase = true};
        
        _sut.Setup(sut => sut.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey).Result)
            .Returns(It.IsAny<ErrorOr<List<Headline>>>());
        
        _sut.Setup(sut => sut.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Returns(_mockHeadlines.Object.First().TranslatedHeadline);
    }
    private readonly Mock<List<Headline>> _mockHeadlines;
    private readonly Mock<TranslatorApi> _mockTranslatorApi;
    private readonly Mock<ITranslatorApiRepository> _mockRepository;
    private readonly Mock<DeeplTranslator> _sut;
     
    [Fact]
    public async Task Translate_WhenInvoked_ShouldreturnErrorOrListHeadline()
    {
        // Assemble
        
        // Act
        var result = await _sut.Object.Translate(_mockHeadlines.Object, _mockTranslatorApi.Object);

        // Assert 
        result.Should().BeOfType<ErrorOr<List<Headline>>>();
    }

    [Fact]
    public async Task Translate_WhenInvoked_WithEmptyHeadlines_ShouldReturnEmptyHeadlineList()
    {
        // Assemble
        _mockHeadlines.Object.Clear();
        
        // Act
        var result = await _sut.Object.Translate([]);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<Headline>>();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Translate_WhenInvokedWith_ShouldSkipRepositoryCallAndGoToTransleHeadlinesMethod()
    {
        // Assemble
        
        // Act
        await _sut.Object.Translate(_mockHeadlines.Object, _mockTranslatorApi.Object);

        // Assert 
        _mockRepository.Verify(repository => repository.Get(1), Times.Never);
        _sut.Verify(sut => sut.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Translate_WhenInvokedWithoutProvidingKey_ShouldCallingRepository()
    {
        // Assemble
        
        // Act
        await _sut.Object.Translate(_mockHeadlines.Object);

        // Assert 
        _mockRepository.Verify(repository => repository.Get(1), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Translate_WhenInvokedWithoutProvidingKeyAndNoSavedKeyInDb_ShouldReturnError()
    {
        // Assemble
        _mockRepository.Reset();

        // Act
        var result = await _sut.Object.Translate(_mockHeadlines.Object);
        
        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.Translator.Service.NoSavedApiKeyFound);
    }

    [Fact]
    public async Task TranslateHeadlines_WhenInvoked_ShouldReturnErrorOrListHeadline()
    {
        // Assemble
        
        // Act
        var result = await _sut.Object.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey);
        
        // Assert 
        result.Should().BeOfType<ErrorOr<List<Headline>>>();
    }

    [Fact]
    public async Task TranslateHeadlines_WhenInvokedWithHeadlineThatIsEmpty_ShouldReturnEmptyTranslatedHeadline()
    {
        // Assemble
        _sut.Reset();
        _sut.Setup(sut => sut.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Returns(_mockHeadlines.Object.First().TranslatedHeadline);
        _mockHeadlines.Object.First().OriginalHeadline = string.Empty;
        
        // Act  
        var result = await _sut.Object.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.First().TranslatedHeadline.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task TranslateHeadlines_WhenInvokedProperly_ShouldCallPerformRequest()
    {
        // Assemble
        _sut.Reset();
        _sut.Setup(sut => sut.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Returns(_mockHeadlines.Object.First().TranslatedHeadline);

        // Act
        await _sut.Object.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey);

        // Assert 
        _sut.Verify(sut => sut.PerformRequest(It.IsAny<string>(),It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task TranslateHeadlines_WhenDeeplProducesAuthException_ShouldWriteExceptionInTranslatedHeadline()
    {
        // Assemble
        _sut.Reset();
        _sut.Setup(sut => sut.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Throws(new AuthorizationException("Authorization failure, check AuthKey"));

        // Act
        var result = await _sut.Object.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.Translator.Api.BadApiKey);
    }
    
    [Fact]
    public async Task TranslateHeadlines_WhenDeeplProducesQuotaException_ShouldWriteExceptionInTranslatedHeadline()
    {
        // Assemble
        _sut.Reset();
        _sut.Setup(sut => sut.PerformRequest(It.IsAny<string>(), It.IsAny<string>()).Result)
            .Throws(new QuotaExceededException("Quota for this billing period has been exceeded"));

        // Act
        var result = await _sut.Object.TranslateHeadlines(_mockHeadlines.Object, _mockTranslatorApi.Object.ApiKey);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.Translator.Api.QuotaExceeded);
    }
}