using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerHelper : TestExternalServiceManagerBase
{
    public TestExternalServiceManagerHelper()
    {
        _sut = new(new Mock<ILogger<ExternalServiceManagerHelper>>().Object, MockServiceProvider.Object,
            MockRepository.Object, MockUow.Object){CallBase = true};

        _mockLlmService = new();
        _mockTranslatorService = new();
        _mockScrapingService = new();
        
        BasicSetup();
    }

    private readonly Mock<ExternalServiceManagerHelper> _sut;
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<ITranslatorService> _mockTranslatorService;
    private readonly Mock<IScrapingService> _mockScrapingService;

    private void BasicSetup()
    {
        _mockLlmService
            .Setup(service => service.ProcessArticleAsync(It.IsAny<ArticleContent>()).Result)
            .Returns(new ErrorOr<ArticleContent>().Value);

        _mockTranslatorService
            .Setup(service => service.Translate(It.IsAny<List<Headline>>()).Result)
            .Returns(new ErrorOr<List<Headline>>().Value);

        _mockScrapingService
            .Setup(service => service.ScrapeWebsiteWithRetry(It.IsAny<NewsOutlet>(), 2).Result)
            .Returns("");
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenNoServiceFound_ReturnsSpecificError()
    {
        // Assemble
        MockServiceProvider.Reset();
        
        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public void FindRequiredExistingService_WhenServiceExists_ReturnsService()
    {
        // Arrange

        // Act
        var result = _sut.Object.FindRequiredExistingService(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestExistingService);
    }

    [Fact]  
    public void FindRequiredExistingService_WhenNoServiceExists_ReturnsError()
    {
        // Assemble
        MockRepository.Reset();

        // Act
        var result = _sut.Object.FindRequiredExistingService(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public void FindRequiredExistingService_WhenNoServiceMatch_ReturnsError()
    {
        // Assemble
        TestServiceDto.ServiceName = "";

        // Act
        var result = _sut.Object.FindRequiredExistingService(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public async Task CheckKeyValidity_WhenProvidedProperLLmService_ReturnsTrue()
    {
        // Assemble

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CheckKeyValidity_WhenProvidedProperTranslatorService_ReturnsTrue()
    {
        // Assemble
        TestService.Type = ExternalServiceType.Translator;
        MockServiceProvider
            .Setup(provider => provider.Provide(TestService.Type, TestService.ApiKey))
            .Returns(_mockTranslatorService.Object);

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenProvidedProperScraperService_ReturnsTrue()
    {
        // Assemble
        TestService.Type = ExternalServiceType.Scraper;
        MockServiceProvider
            .Setup(provider => provider.Provide(TestService.Type, TestService.ApiKey))
            .Returns(_mockScrapingService.Object);

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CheckKeyValidity_WhenProvidedFaultyLlmService_ReturnsFalse()
    {
        // Assemble
        MockLlmService
            .Setup(service => service.ProcessArticleAsync(It.IsAny<ArticleContent>()).Result)
            .Returns(Errors.ExternalServiceProvider.Llm.AssistantRunError);

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Llm.AssistantRunError);
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenProvidedFaultyTranslatorService_ReturnsFalse()
    {
        // Assemble
        TestService.Type = ExternalServiceType.Translator;
        _mockTranslatorService
            .Setup(service => service.Translate(It.IsAny<List<Headline>>()).Result)
            .Returns(Errors.Translator.Api.QuotaExceeded);
        MockServiceProvider
            .Setup(x => x.Provide(TestService.Type, TestService.ApiKey))
            .Returns(_mockTranslatorService.Object);
        

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Translator.Api.QuotaExceeded);
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenProvidedFaultyScraperService_ReturnsFalse()
    {
        // Assemble
        TestService.Type = ExternalServiceType.Scraper;
        _mockScrapingService
            .Setup(service => service.ScrapeWebsiteWithRetry(It.IsAny<NewsOutlet>(), 2).Result)
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
        MockServiceProvider
            .Setup(x => x.Provide(TestService.Type, TestService.ApiKey))
            .Returns(_mockScrapingService.Object);
        

        // Act
        var result = await _sut.Object.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }
}