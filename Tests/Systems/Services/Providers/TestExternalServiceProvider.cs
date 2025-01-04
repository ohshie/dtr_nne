using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.Providers;

[Experimental("OPENAI001")]
public class TestExternalServiceProvider
{
    public TestExternalServiceProvider()
    {
        _mockOpenAiService = new();
        _mockDeeplService = new();
        _mockRepository = new();
        _mockExternalServiceLlmList = new();
        _mockServiceFactory = new();
        _mockExternalServiceTranslatorList = new();
        
        _mockExternalServiceLlmList.Object.Add(new ExternalService
        {
            InUse = true,
            ServiceName = Enum.GetName(typeof(LlmServiceType), LlmServiceType.OpenAi)!,
            Type = ExternalServiceType.Llm,
        });
        
        _mockExternalServiceTranslatorList.Object.Add(new ExternalService
        {
            InUse = true,
            ServiceName = Enum.GetName(typeof(TranslatorServiceType), TranslatorServiceType.Deepl)!,
            Type = ExternalServiceType.Translator
        });
        
        _mockServiceFactory
            .Setup(factory => factory.CreateOpenAiService(It.IsAny<ExternalService>()))
            .Returns(_mockOpenAiService.Object);
        
        _mockServiceFactory
            .Setup(factory => factory.CreateDeeplService(It.IsAny<ExternalService>()))
            .Returns(_mockDeeplService.Object);

        _mockRepository
            .Setup(repository => repository.GetByType(ExternalServiceType.Llm))
            .Returns(_mockExternalServiceLlmList.Object);
        
        _mockRepository
            .Setup(repository => repository.GetByType(ExternalServiceType.Translator))
            .Returns(_mockExternalServiceTranslatorList.Object);
        
        _sut = new(new Mock<ILogger<ExternalServiceProvider>>().Object, _mockRepository.Object, _mockServiceFactory.Object);
    }

    private readonly ExternalServiceProvider _sut;
    private readonly Mock<IOpenAiService> _mockOpenAiService;
    private readonly Mock<IDeeplService> _mockDeeplService;
    private readonly Mock<IExternalServiceProviderRepository> _mockRepository;
    private readonly Mock<IExternalServiceFactory> _mockServiceFactory;
    private readonly Mock<List<ExternalService>> _mockExternalServiceLlmList;
    private readonly Mock<List<ExternalService>> _mockExternalServiceTranslatorList;

    [Fact]
    public void GetService_Llm_WhenSuccess_ShouldReturnRequestedService()
    {
        // Assemble
        
        // Act
        var result = _sut.Provide(ExternalServiceType.Llm, "test");

        // Assert
        result.Should().BeAssignableTo<ILlmService>();
    }
    
    [Fact]
    public void GetService_Translator_WhenSuccess_ShouldReturnRequestedService()
    {
        // Assemble
        
        // Act
        var result = _sut.Provide(ExternalServiceType.Translator, "test");

        // Assert
        result.Should().BeAssignableTo<ITranslatorService>();
    }

    [Fact]
    public void GetService_OnInvoke_ShouldCallRepositoryForCurrenActiveService()
    {
        // Assemble

        // Act
        _sut.Provide(ExternalServiceType.Llm);

        // Assert 
        _mockRepository.Verify(repository => repository.GetByType(ExternalServiceType.Llm), Times.Once);
    }

    [Fact]
    public void GetService_IfNoRelevantServiceFoundInDb_ShouldThrow()
    {
        // Assemble
        _mockRepository.Reset();

        // Act
        var act = () => _sut.Provide(ExternalServiceType.Llm);

        // Assert 
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetService_IfNoEnabledServiceFound_ShouldThrow()
    {
        // Assemble
        _mockExternalServiceLlmList.Object.Clear();
        _mockExternalServiceLlmList.Object.Add(new ExternalService
        {
            InUse = false,
            ServiceName = Enum.GetName(typeof(LlmServiceType), LlmServiceType.OpenAi)!,
            Type = ExternalServiceType.Llm,
        });
        
        // Act
        var act = () => _sut.Provide(ExternalServiceType.Llm);

        // Assert 
        act.Should().Throw<InvalidOperationException>();
    }
}