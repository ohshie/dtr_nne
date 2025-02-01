using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerHelper : TestExternalServiceManagerBase
{
    public TestExternalServiceManagerHelper()
    {
        _sut = new(Substitute.For<ILogger<ExternalServiceManagerHelper>>(), 
            MockServiceProvider,
            MockRepository, MockUow);

        _mockLlmService = Substitute.For<ILlmService>();
        _mockTranslatorService = Substitute.For<ITranslatorService>();
        _mockScrapingService = Substitute.For<IScrapingService>();
        
        BasicSetup();
    }

    private readonly ExternalServiceManagerHelper _sut;
    private readonly ILlmService _mockLlmService;
    private readonly ITranslatorService _mockTranslatorService;
    private readonly IScrapingService _mockScrapingService;

    private void BasicSetup()
    {
        _mockLlmService.
            ProcessArticleAsync(Arg.Any<ArticleContent>())
            .Returns(new ErrorOr<ArticleContent>().Value);

        _mockTranslatorService
            .Translate(Arg.Any<List<Headline>>())
            .Returns(new ErrorOr<List<Headline>>().Value);
            
        _mockScrapingService
            .ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .Returns("");
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenNoServiceFound_ReturnsSpecificError()
    {
        // Assemble
        MockServiceProvider
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns((IExternalService)null!);
        
        // Act
        var result = await _sut.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public void FindRequiredExistingService_WhenServiceExists_ReturnsService()
    {
        // Arrange

        // Act
        var result = _sut.FindRequiredExistingService(TestService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestExistingService);
    }

    [Fact]  
    public void FindRequiredExistingService_WhenNoServiceExists_ReturnsError()
    {
        // Assemble
        MockRepository.ClearSubstitute();

        // Act
        var result = _sut.FindRequiredExistingService(TestService);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public void FindRequiredExistingService_WhenNoServiceMatch_ReturnsError()
    {
        // Assemble
        TestService.ServiceName = "";

        // Act
        var result = _sut.FindRequiredExistingService(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public async Task CheckKeyValidity_WhenProvidedProperLLmService_ReturnsTrue()
    {
        // Assemble

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

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
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns(_mockTranslatorService);

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

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
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns(_mockScrapingService);

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CheckKeyValidity_WhenProvidedFaultyLlmService_ReturnsFalse()
    {
        // Assemble
        MockLlmService
            .ProcessArticleAsync(Arg.Any<ArticleContent>())
            .Returns(Errors.ExternalServiceProvider.Llm.AssistantRunError);

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

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
            .Translate(Arg.Any<List<Headline>>())
            .Returns(Errors.Translator.Api.QuotaExceeded);
        MockServiceProvider
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns(_mockTranslatorService);        

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

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
            .ScrapeWebsiteWithRetry(Arg.Any<NewsOutlet>())
            .Returns(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
        MockServiceProvider
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns(_mockScrapingService);

        // Act
        var result = await _sut.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Scraper.ScrapingRequestError(""));
    }

    [Fact]
    public async Task PerformDataOperations_WhenUpdateSuccess_ReturnsTrue()
    {
        // Assemble

        // Act
        var result = await _sut.PerformDataOperation(TestService, "update");

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenDeleteSuccess_ReturnsTrue()
    {
        // Assemble
        this.MockRepository.Remove(TestService)
            .Returns(true);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "delete");

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenAddSuccess_ReturnsTrue()
    {
        // Assemble
        this.MockRepository.Add(TestService)
            .Returns(true);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "add");

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenUpdateFails_ReturnsFalse()
    {
        // Assemble
        this.MockRepository.Update(TestService)
            .Returns(false);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "update");

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UpdatingDbFailed);
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenDeleteFails_ReturnsFalse()
    {
        // Assemble
        this.MockRepository.Remove(TestService)
            .Returns(false);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "delete");

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.RemovingFailed);
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenAddFails_ReturnsFalse()
    {
        // Assemble
        this.MockRepository.Add(TestService)
            .Returns(false);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "add");

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
    
    [Fact]
    public async Task PerformDataOperations_WhenOuWFails_ReturnsError()
    {
        // Assemble
        MockUow
            .Save()
            .Returns(false);

        // Act
        var result = await _sut.PerformDataOperation(TestService, "update");

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UnitOfWorkSaveFailed);
    }
}