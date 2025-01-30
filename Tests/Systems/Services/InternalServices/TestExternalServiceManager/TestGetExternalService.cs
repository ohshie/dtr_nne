using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestGetExternalService : TestExternalServiceManagerBase
{
    public TestGetExternalService()
    {
        _sut = new(Substitute.For<ILogger<GetExternalService>>(), 
            MockMapper, MockRepository);
        
        _testExternalServicesList = [TestService];
        _testExternalServicesDtos = [TestServiceDto];
    }

    private readonly List<ExternalService> _testExternalServicesList;
    private readonly List<ExternalServiceDto> _testExternalServicesDtos;
    
    private readonly GetExternalService _sut;

    [Fact]
    public void Get_WhenServicesRegistered_ReturnsListOfServices()
    {
        // Assemble
        MockRepository
            .GetByType(ExternalServiceType.Llm)
            .Returns(_testExternalServicesList);
        MockMapper
            .ServiceToDto(_testExternalServicesList)
            .Returns(_testExternalServicesDtos);
 
        _testExternalServicesDtos[0].Id = _testExternalServicesList[0].Id;

        // Act
        var result = _sut.GetAllByType(ExternalServiceType.Llm);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testExternalServicesList);
    }

    [Fact]
    public void Get_WhenNoServicesRegistered_ReturnsError()
    {
        // Assemble
        MockRepository.ClearSubstitute();

        // Act
        var result = _sut.GetAllByType(ExternalServiceType.Llm);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
}