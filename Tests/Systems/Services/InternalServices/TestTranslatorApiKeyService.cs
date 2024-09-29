using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services;

public class TestTranslatorApiKeyService(TranslatorApiKeyServiceFixture apiKeyServiceFixture) 
    : IClassFixture<TranslatorApiKeyServiceFixture>
{
    [Fact]
    public async Task Add_OnSuccess_ShouldReturnProvidedApiKey()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        var providedApiKey = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        providedApiKey.IsError
            .Should()
            .BeFalse();
        providedApiKey.Value.ApiKey
            .Should()
            .BeEquivalentTo(apiKeyServiceFixture.MockApiKeyDto.Object.ApiKey);
    }

    [Fact]
    public async Task Add_WhenProvidedWithKey_ShouldCallIMapperAndITranslatorServiceTranslate()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();

        // Act
        await apiKeyServiceFixture.Sut
            .Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        apiKeyServiceFixture.MockApiKeyMapper
            .Verify(mapper => mapper.MapTranslatorApiDtoToTranslatorApi(apiKeyServiceFixture.MockApiKeyDto.Object), Times.AtLeastOnce);
        apiKeyServiceFixture.MockTranslatorService
            .Verify(service => service.Translate(It.IsAny<List<Headline>>(), It.IsAny<TranslatorApi>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Add_WhenInvokedWithBadKey_ShouldReturnError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockTranslatorService
            .Setup(service =>
                service.Translate(It.IsAny<List<Headline>>(), apiKeyServiceFixture.MockApiKey.Object).Result)
            .Returns(Errors.Translator.Api.BadApiKey);
        
        // Act
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);
        
        // Assert 
        result.IsError
            .Should()
            .BeTrue();
        result.FirstError
            .Should()
            .BeEquivalentTo(Errors.Translator.Api.BadApiKey);
    }

    [Fact]
    public async Task Add_WhenInvokedProperly_ShouldCallIRepositoryTranslatorApiAdd()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        apiKeyServiceFixture.MockTranslatorRepository
            .Verify(repository => repository.Add(apiKeyServiceFixture.MockApiKey.Object), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Add_WhenInvokedProperly_IfAddingToRepositoryFails_ReturnsError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
       apiKeyServiceFixture.MockTranslatorRepository
            .Setup(repository => repository.Add(apiKeyServiceFixture.MockApiKey.Object).Result)
            .Returns(false);

        // Act
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.Translator.Api.AddingFailed);
    }

    [Fact]
    public async Task Add_WhenInvokedProperly_ShouldCallUnitOfWorkSave()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        apiKeyServiceFixture.MockUnitOfWork.Verify(work => work.Save(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Add_WhenInvokedProperly_IfUnitOfWorkSaveFails_ShouldReturnUoWDbError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockUnitOfWork
            .Setup(uow => uow.Save().Result)
            .Returns(false);

        // Act
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockApiKeyDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.DbErrors.UnitOfWorkSaveFailed);
    }
}