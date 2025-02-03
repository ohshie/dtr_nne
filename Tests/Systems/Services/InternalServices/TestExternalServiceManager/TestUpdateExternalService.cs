using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestUpdateExternalService : TestExternalServiceManagerBase
{
    public TestUpdateExternalService()
    {
        _sut = new(Substitute.For<ILogger<UpdateExternalService>>(),
            MockHelper, MockMapper);
    }
    
    private readonly UpdateExternalService _sut;
    
    [Fact]
    public async Task UpdateKey_WhenValidService_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await _sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }

    [Fact]
    public async Task UpdateKey_WhenCheckKeyFail_ReturnsError()
    {
        // Assemble
        MockHelper
            .CheckKeyValidity(TestService)
            .Returns(Errors.ExternalServiceProvider.Service.BadApiKey);
        
        // Act
        var result = await _sut.Update(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.BadApiKey);
    }
    
    [Fact]
    public async Task UpdateKey_WhenServiceNotFound_ReturnsError()
    {
        // Arrange
        MockHelper
            .FindRequiredExistingService(TestService)
            .Returns(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
        
        // Act
        var result = await _sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public async Task Update_WhenDbOperationFails_ReturnsError()
    {
        // Arrange
        MockHelper
            .PerformDataOperation(TestService, "update")
            .Returns(false);
        
        // Act
        var result = await _sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UpdatingDbFailed);
    }
}