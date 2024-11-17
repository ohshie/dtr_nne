using dtr_nne.Application.DTO.Llm;
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

public class TestLlmApiKeyService
{
    public TestLlmApiKeyService()
    {
        MockLlmService = new ();
        MockLlmServiceProvider = new();
        MockApiKeyMapper = new ();
        MockLlmApiRepository = new();
        MockApiKeyDto = new();
        MockApiKey = new();
        MockUnitOfWork = new Mock<IUnitOfWork<NneDbContext>>();
        MockArticle = new Mock<Article>();
        
        CreateDefaultStatus();
        
        Sut = new(logger: new Mock<ILogger<LlmApiKeyService>>().Object, 
            mapper: MockApiKeyMapper.Object, 
            llmServiceProvider: MockLlmServiceProvider.Object);
    }

    public Mock<Article> MockArticle { get; }
    internal Mock<IUnitOfWork<NneDbContext>> MockUnitOfWork { get; }
    internal Mock<IExternalServiceProvider> MockLlmServiceProvider { get; }
    private Mock<LlmApi> MockApiKey { get; }
    internal Mock<LlmApiDto> MockApiKeyDto { get; }
    internal Mock<ILlmService> MockLlmService { get; }
    internal Mock<IExternalServiceMapper> MockApiKeyMapper { get; }
    internal Mock<ILlmApiRepository> MockLlmApiRepository { get; }
    internal LlmApiKeyService Sut { get; }
    
    [Fact]
    public async Task Add_OnSuccess_ShouldReturnProvidedApiKey()
    {
        // Assemble
        
        // Act
        var providedApiKey = await Sut.Add(MockApiKeyDto.Object);

        // Assert 
        providedApiKey.IsError
            .Should()
            .BeFalse();
        providedApiKey.Value.ApiKey
            .Should()
            .BeEquivalentTo(MockApiKeyDto.Object.ApiKey);
    }
    
    [Fact]
    public async Task Add_WhenProvidedWithKey_ShouldCallIMapperAndILlmServiceRewrite()
    {
        // Assemble

        // Act
        await Sut
            .Add(MockApiKeyDto.Object);

        // Assert 
        MockApiKeyMapper
            .Verify(mapper => mapper.MapLlmApiDtoToLlmApi(MockApiKeyDto.Object), Times.AtLeastOnce);
        MockLlmService
            .Verify(service => service.RewriteAsync(It.IsAny<Article>(), MockApiKey.Object), Times.Once);
    }

    private void CreateDefaultStatus()
    {
        MockApiKeyMapper
            .Setup(mapper => mapper.MapLlmApiDtoToLlmApi(MockApiKeyDto.Object))
            .Returns(MockApiKey.Object);

        MockLlmServiceProvider
            .Setup(provider => provider.GetService(ExternalServiceType.Llm).Result)
            .Returns(MockLlmService.Object);

        MockLlmService
            .Setup(service => service.RewriteAsync(It.IsAny<Article>(), MockApiKey.Object).Result)
            .Returns(It.IsAny<Article>());
    }
}