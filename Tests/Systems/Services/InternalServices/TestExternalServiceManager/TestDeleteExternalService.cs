using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestDeleteExternalService : TestExternalServiceManagerBase
{
    public TestDeleteExternalService()
    {
        _sut = new(Substitute.For<ILogger<DeleteExternalService>>(), 
            MockMapper,
            MockHelper);
    }
    
    private readonly DeleteExternalService _sut;

    [Fact]
    public async Task Delete_WhenProperRequest_ReturnsDeletedServiceDto()
    {
        // Assemble

        // Act
        var result = await _sut.Delete(TestBaseExternalServiceDto);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }

    [Fact]
    public async Task Delete_WhenExistingServiceNotFound_ReturnsError()
    {
        // Assemble
        MockHelper
            .FindRequiredExistingService(TestService)
            .Returns(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);

        // Act
        var result = await _sut.Delete(TestBaseExternalServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public async Task Delete_WhenDataOperationFails_ReturnsError()
    {
        // Assemble
        MockHelper
            .PerformDataOperation(TestService, "delete")
            .Returns(Errors.DbErrors.RemovingFailed);

        // Act
        var result = await _sut.Delete(TestBaseExternalServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.RemovingFailed);
    }
}