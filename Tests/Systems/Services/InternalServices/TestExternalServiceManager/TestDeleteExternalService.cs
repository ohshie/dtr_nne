using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestDeleteExternalService : TestExternalServiceManagerBase
{
    public TestDeleteExternalService()
    {
        _sut = new(new Mock<ILogger<DeleteExternalService>>().Object,
            MockHelper.Object);
    }
    
    private readonly DeleteExternalService _sut;

    [Fact]
    public async Task Delete_WhenProperRequest_ReturnsDeletedServiceDto()
    {
        // Assemble

        // Act
        var result = await _sut.Delete(TestServiceDto);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }

    [Fact]
    public async Task Delete_WhenExistingServiceNotFound_ReturnsError()
    {
        // Assemble
        MockHelper
            .Setup(helper => helper.FindRequiredExistingService(TestServiceDto))
            .Returns(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);

        // Act
        var result = await _sut.Delete(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }

    [Fact]
    public async Task Delete_WhenDataOperationFails_ReturnsError()
    {
        // Assemble
        MockHelper
            .Setup(helper => helper.PerformDataOperation(TestService, "delete").Result)
            .Returns(Errors.DbErrors.RemovingFailed);

        // Act
        var result = await _sut.Delete(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.RemovingFailed);
    }
}