using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestExternalServiceProviderRepository : IClassFixture<GenericDatabaseFixture<ExternalService>>
{
    public TestExternalServiceProviderRepository(GenericDatabaseFixture<ExternalService> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        _mockExternalServiceCollection = new();
        _mockExternalService = new();
        
        var logger = new Mock<ILogger<ExternalServiceProviderRepository>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, 
            new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);
        
        _sut = new(logger.Object, unitOfWork);
    }

    private readonly ExternalServiceProviderRepository _sut;
    private readonly GenericDatabaseFixture<ExternalService> _genericDatabaseFixture;
    private readonly Mock<IEnumerable<ExternalService>> _mockExternalServiceCollection;
    private readonly Mock<ExternalService> _mockExternalService;

    [Fact]
    public async Task GetByType_WhenInvoked_ShouldReturnIEnumerableExternalService()
    {
        // Assemble
        _mockExternalService.Object.Type = ExternalServiceType.Llm;
        _mockExternalService.Object.InUse = true;

        _genericDatabaseFixture.Context.ExternalServices.Add(_mockExternalService.Object);

        // Act
        var result = await _sut.GetByType(ExternalServiceType.Llm);

        // Assert 
        result.Should().HaveCount(1);
    }
}