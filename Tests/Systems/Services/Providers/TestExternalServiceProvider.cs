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
        _mockServiceProvider = new();
        _mockOpenAiService = new();
        _mockRepository = new();
        _mockExternalServiceList = new();
        _mockServiceFactory = new();
        
        _mockExternalServiceList.Object.Add(new ExternalService
        {
            InUse = true,
            ServiceName = Enum.GetName(typeof(LlmServiceType), LlmServiceType.OpenAi)!,
            Type = ExternalServiceType.Llm,
        });

        _mockServiceProvider
            .Setup(provider => provider.GetService(typeof(IOpenAiService)))
            .Returns(_mockOpenAiService.Object);

        _mockRepository
            .Setup(repository => repository.GetByType(ExternalServiceType.Llm))
            .Returns(_mockExternalServiceList.Object);
        
        _sut = new(new Mock<ILogger<ExternalServiceProvider>>().Object, _mockServiceProvider.Object, _mockRepository.Object, _mockServiceFactory.Object);
    }

    private readonly ExternalServiceProvider _sut;
    private readonly Mock<IOpenAiService> _mockOpenAiService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IExternalServiceProviderRepository> _mockRepository;
    private readonly Mock<IExternalServiceFactory> _mockServiceFactory;
    private readonly Mock<List<ExternalService>> _mockExternalServiceList;

    [Fact]
    public void GetService_WhenSuccessfull_ShouldReturnRequestedService()
    {
        // Assemble
        
        // Act
        var result = _sut.Provide(ExternalServiceType.Llm);

        // Assert
        result.Should().BeAssignableTo<ILlmService>();
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
        _mockExternalServiceList.Object.Clear();
        _mockExternalServiceList.Object.Add(new ExternalService
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