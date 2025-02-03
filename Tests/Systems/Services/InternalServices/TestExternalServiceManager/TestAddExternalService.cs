using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestAddExternalService : TestExternalServiceManagerBase
{
    public TestAddExternalService()
    {
        _sut = new AddExternalService(
            Substitute.For<ILogger<AddExternalService>>(), 
            MockMapper, 
            MockHelper);
    }

    private readonly AddExternalService _sut;
    
    [Fact]
    public async Task Add_WhenValidService_ReturnsSuccess()
    {
        // Arrange
        MockServiceProvider
            .Provide(ExternalServiceType.Llm, TestService.ApiKey)
            .Returns(MockLlmService);
        
        // Act
        var result = await _sut.Add(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }
    
    [Fact]
    public async Task Add_WhenKeyValidationFails_ReturnsError()
    {
        // Arrange
        MockHelper
            .CheckKeyValidity(TestService)
            .Returns(Errors.ExternalServiceProvider.Service.BadApiKey);
            
        // Act
        var result = await _sut.Add(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.BadApiKey);
    }
    
    [Fact]
    public async Task Add_WhenDbOperationFails_ReturnsError()
    {
        // Arrange
        MockHelper
            .PerformDataOperation(TestService, "add")
            .Returns(false);
        
        // Act
        var result = await _sut.Add(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
}