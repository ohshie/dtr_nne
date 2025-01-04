using System.Diagnostics.CodeAnalysis;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Infrastructure.ExternalServices;
using dtr_nne.Infrastructure.ExternalServices.LlmServices;
using dtr_nne.Infrastructure.ExternalServices.TranslatorServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.Providers;

[Experimental("OPENAI001")]
public class TestExternalServiceFactory
{
    public TestExternalServiceFactory()
    {
        _mockServiceProvider = new();
        _mockLogger = new();
        _mockOpenAiRepository = new();
        _mockTranslatorLogger = new();

        _mockExternalService = new()
        {
            InUse = true
        };

        BasicSetup();
        
        _sut = new(_mockServiceProvider.Object);
    }
    
    private readonly ExternalServiceFactory _sut;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<OpenAiService>> _mockLogger;
    private readonly Mock<IOpenAiAssistantRepository> _mockOpenAiRepository;
    private readonly Mock<ILogger<DeeplTranslator>> _mockTranslatorLogger;
    private readonly ExternalService _mockExternalService;

    private void BasicSetup()
    {
        _mockServiceProvider
            .Setup(provider => provider.GetService(typeof(ILogger<OpenAiService>)))
            .Returns(_mockLogger.Object);
            
        _mockServiceProvider
            .Setup(provider => provider.GetService(typeof(IOpenAiAssistantRepository)))
            .Returns(_mockOpenAiRepository.Object);
            
        _mockServiceProvider
            .Setup(provider => provider.GetService(typeof(ILogger<DeeplTranslator>)))
            .Returns(_mockTranslatorLogger.Object);
    }
    
    [Fact]
    public void CreateOpenAiService_WhenCalled_ShouldReturnOpenAiService()
    {
        // Assemble
        
        // Act
        var result = _sut.CreateOpenAiService(_mockExternalService);

        // Assert
        result.Should().BeOfType<OpenAiService>();
    }
    
    [Fact]
    public void CreateOpenAiService_WhenCalled_ShouldGetRequiredServices()
    {
        // Assemble
        
        // Act
        _ = _sut.CreateOpenAiService(_mockExternalService);

        // Assert
        _mockServiceProvider.Verify(
            provider => provider.GetService(typeof(ILogger<OpenAiService>)),
            Times.Once);
        _mockServiceProvider.Verify(
            provider => provider.GetService(typeof(IOpenAiAssistantRepository)),
            Times.Once);
    }
    
    [Fact]
    public void CreateDeeplService_WhenCalled_ShouldReturnDeeplTranslator()
    {
        // Assemble
        
        // Act
        var result = _sut.CreateDeeplService(_mockExternalService);

        // Assert
        result.Should().BeOfType<DeeplTranslator>();
    }

    [Fact]
    public void CreateDeeplService_WhenCalled_ShouldGetRequiredServices()
    {
        // Assemble
        
        // Act
        _ = _sut.CreateDeeplService(_mockExternalService);

        // Assert
        _mockServiceProvider.Verify(
            provider => provider.GetService(typeof(ILogger<DeeplTranslator>)),
            Times.Once);
    }
}