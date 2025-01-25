using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestUpdateExternalService : TestExternalServiceManagerBase
{
    public TestUpdateExternalService()
    {
        _sut = new(new Mock<ILogger<UpdateExternalService>>().Object,
            MockHelper.Object, MockMapper.Object);
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
            .Setup(helper => helper.CheckKeyValidity(TestService).Result)
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
            .Setup(helper => helper.FindRequiredExistingService(TestService))
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
            .Setup(helper => helper.PerformDataOperation(TestService, "update").Result)
            .Returns(false);
        
        // Act
        var result = await _sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UpdatingDbFailed);
    }
}