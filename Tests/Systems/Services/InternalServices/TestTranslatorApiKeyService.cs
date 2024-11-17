using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using ErrorOr;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices;

public class TestTranslatorApiKeyService(TranslatorApiKeyServiceFixture apiKeyServiceFixture) 
    : IClassFixture<TranslatorApiKeyServiceFixture>
{
    [Fact]
    public async Task Add_OnSuccess_ShouldReturnProvidedApiKey()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        var providedApiKey = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        providedApiKey.IsError
            .Should()
            .BeFalse();
        providedApiKey.Value.ApiKey
            .Should()
            .BeEquivalentTo(apiKeyServiceFixture.MockExternalServiceDto.Object.ApiKey);
    }

    [Fact]
    public async Task Add_WhenProvidedWithKey_ShouldCallIMapperAndITranslatorServiceTranslate()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();

        // Act
        await apiKeyServiceFixture.Sut
            .Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        apiKeyServiceFixture.MockApiKeyMapper
            .Verify(mapper => mapper.DtoToService(apiKeyServiceFixture.MockExternalServiceDto.Object), Times.AtLeastOnce);
        apiKeyServiceFixture.MockTranslatorService
            .Verify(service => service.Translate(It.IsAny<List<Headline>>(), It.IsAny<ExternalService>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Add_WhenInvokedWithBadKey_ShouldReturnError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockTranslatorService
            .Setup(service =>
                service.Translate(It.IsAny<List<Headline>>(), apiKeyServiceFixture.MockExternalService.Object).Result)
            .Returns(Errors.Translator.Api.BadApiKey);
        
        // Act
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);
        
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
        await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        apiKeyServiceFixture.MockTranslatorRepository
            .Verify(repository => repository.Add(apiKeyServiceFixture.MockExternalService.Object), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Add_WhenInvokedProperly_IfAddingToRepositoryFails_ReturnsError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
       apiKeyServiceFixture.MockTranslatorRepository
            .Setup(repository => repository.Add(apiKeyServiceFixture.MockExternalService.Object).Result)
            .Returns(false);

        // Act
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

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
        await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

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
        var result = await apiKeyServiceFixture.Sut.Add(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.DbErrors.UnitOfWorkSaveFailed);
    }

    [Fact]
    public async Task UpdateKey_OnSuccess_ShouldReturnKey()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        var result = await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<ErrorOr<ExternalServiceDto>>();
    }

    [Fact]
    public async Task UpdateKey_WhenProvidedWithKey_ShouldCallIMapAndCheckApiKey()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        await apiKeyServiceFixture.Sut
            .UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        apiKeyServiceFixture.MockApiKeyMapper
            .Verify(mapper => mapper.DtoToService(apiKeyServiceFixture.MockExternalServiceDto.Object), Times.AtLeastOnce);
        apiKeyServiceFixture.MockTranslatorService
            .Verify(service => service.Translate(It.IsAny<List<Headline>>(), It.IsAny<ExternalService>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_WhenInvokedProperly_ShouldCallIRepositoryTranslatorApiUpdate()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        apiKeyServiceFixture.MockTranslatorRepository
            .Verify(repository => repository.Update(apiKeyServiceFixture.MockExternalService.Object), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task UpdateKey_WhenInvokedProperly_IfUpdatingFails_ReturnsError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockTranslatorRepository
            .Setup(repository => repository.Update(apiKeyServiceFixture.MockExternalService.Object))
            .Returns(false);

        // Act
        var result = await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.Translator.Api.UpdatingFailed);
    }

    [Fact]
    public async Task UpdateKey_WhenInvokedProperly_ShouldCallUnitOfWorkSave()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        
        // Act
        await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        apiKeyServiceFixture.MockUnitOfWork.Verify(work => work.Save(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_WhenInvokedProperly_IfUnitOfWorkSaveFails_ShouldReturnUoWDbError()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockUnitOfWork
            .Setup(uow => uow.Save().Result)
            .Returns(false);

        // Act
        var result = await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.DbErrors.UnitOfWorkSaveFailed);
    }

    [Fact]
    public async Task UpdateKey_WhenInvokedProperly_ShouldGetCurrentApiKey_And_ReturnErrrorIfNoCurrentKeyFound()
    {
        // Assemble
        apiKeyServiceFixture.ResetMockState();
        apiKeyServiceFixture.MockTranslatorRepository
            .Setup(repository => repository.Get(1).Result)
            .Returns(It.IsAny<ExternalService>());

        // Act
        var result = await apiKeyServiceFixture.Sut.UpdateKey(apiKeyServiceFixture.MockExternalServiceDto.Object);
        
        // Assert
        apiKeyServiceFixture.MockTranslatorRepository.Verify(repository => repository.Get(1), Times.AtLeastOnce);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Service.NoSavedApiKeyFound);
    }
}