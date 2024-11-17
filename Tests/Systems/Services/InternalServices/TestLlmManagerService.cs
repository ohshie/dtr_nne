using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.ExternalServices.LlmServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices;

public class TestLlmManagerService
{
    public TestLlmManagerService()
    {
        MockLlmService = new ();
        MockLlmServiceProvider = new();
        MockApiKeyMapper = new ();
        MockRepository = new();
        MockExternalServiceDto = new();
        MockExternalService = new();
        MockUnitOfWork = new Mock<IUnitOfWork<NneDbContext>>();
        MockArticle = new Mock<Article>();
        
        CreateDefaultStatus();
        
        Sut = new(logger: new Mock<ILogger<LlmManagerService>>().Object, 
            repository: MockRepository.Object,
            mapper: MockApiKeyMapper.Object, 
            serviceProvider: MockLlmServiceProvider.Object);
    }

    public Mock<Article> MockArticle { get; }
    internal Mock<IUnitOfWork<NneDbContext>> MockUnitOfWork { get; }
    internal Mock<IExternalServiceProvider> MockLlmServiceProvider { get; }
    private Mock<ExternalService> MockExternalService { get; }
    internal Mock<ExternalServiceDto> MockExternalServiceDto { get; }
    internal Mock<ILlmService> MockLlmService { get; }
    internal Mock<IExternalServiceMapper> MockApiKeyMapper { get; }
    internal Mock<IExternalServiceProviderRepository> MockRepository { get; }
    internal LlmManagerService Sut { get; }

    [Fact]
    public async Task CheckApiKey_OnGetServiceReturningNull_ShouldReturnErrorNoSavedFound()
    {
        // Assemble
        MockLlmServiceProvider.Reset();

        // Act
        var result = await Sut.CheckApiKey(MockExternalService.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Service.NoSavedApiKeyFound);
    }
    
    [Fact]
    public async Task CheckApiKey_OnApiKeyFailure_ShouldReturnErrorBadApiKey()
    {
        // Assemble
        MockLlmService
            .Setup(service => service.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()).Result)
            .Returns(Errors.ExternalServiceProvider.Service.BadApiKey);

        // Act
        var result = await Sut.CheckApiKey(MockExternalService.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Service.BadApiKey);
    }
    
    [Fact]
    public async Task Add_OnSuccess_ShouldReturnProvidedApiKey()
    {
        // Assemble
        
        // Act
        var providedApiKey = await Sut.Add(MockExternalServiceDto.Object);

        // Assert 
        providedApiKey.IsError
            .Should()
            .BeFalse();
        providedApiKey.Value.ApiKey
            .Should()
            .BeEquivalentTo(MockExternalServiceDto.Object.ApiKey);
    }
    
    [Fact]
    public async Task Add_WhenProvidedWithKey_ShouldCallIMapperAndILlmServiceRewrite()
    {
        // Assemble

        // Act
        await Sut
            .Add(MockExternalServiceDto.Object);

        // Assert 
        MockApiKeyMapper
            .Verify(mapper => mapper.DtoToService(MockExternalServiceDto.Object), Times.AtLeastOnce);
        MockLlmService
            .Verify(service => service.ProcessArticleAsync(It.IsAny<Article>(), MockExternalService.Object.ApiKey), Times.Once);
    }

    [Fact]
    public async Task Add_OnKeyVerificationError_ShouldReturn()
    {
        // Assemble
        MockLlmService
            .Setup(service => service.ProcessArticleAsync(It.IsAny<Article>(), MockExternalService.Object.ApiKey).Result)
            .Returns(ErrorOr.Error.Validation());

        // Act
        var result = await Sut.Add(MockExternalServiceDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Service.BadApiKey);
    }

    private void CreateDefaultStatus()
    {
        MockApiKeyMapper
            .Setup(mapper => mapper.DtoToService(MockExternalServiceDto.Object))
            .Returns(MockExternalService.Object);

        MockLlmServiceProvider
            .Setup(provider => provider.GetService(ExternalServiceType.Llm).Result)
            .Returns(MockLlmService.Object);

        MockLlmService
            .Setup(service => service.ProcessArticleAsync(It.IsAny<Article>(), MockExternalService.Object.ApiKey).Result)
            .Returns(It.IsAny<Article>());
    }
}