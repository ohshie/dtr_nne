using dtr_nne.Application.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Systems.ServiceCollections;

public class TestInfrastructureServiceCollection
{
    public TestInfrastructureServiceCollection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        // Create configuration with test connection string
        var configValues = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", connection.ConnectionString}
        };

        new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
        
        _sut = CreateServiceCollection();
    }

    private IServiceCollection _sut;
    
    private IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddInfrastructure(configuration);
        return services;
    }
    
    [Fact]
    public void ShouldRegisterDbContext()
    {
        // Arrange

        // Act
        var serviceProvider = _sut.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<INneDbContext>();
        var concreteDbContext = serviceProvider.GetService<NneDbContext>();

        // Assert
        dbContext.Should().NotBeNull();
        concreteDbContext.Should().NotBeNull();
    }
    
    [Theory]
    [InlineData(typeof(IUnitOfWork<INneDbContext>), ServiceLifetime.Scoped)]
    [InlineData(typeof(IUnitOfWork<NneDbContext>), ServiceLifetime.Scoped)]
    [InlineData(typeof(INewsOutletRepository), ServiceLifetime.Scoped)]
    [InlineData(typeof(IExternalServiceProviderRepository), ServiceLifetime.Scoped)]
    [InlineData(typeof(IOpenAiAssistantRepository), ServiceLifetime.Scoped)]
    [InlineData(typeof(IExternalServiceProvider), ServiceLifetime.Transient)]
    [InlineData(typeof(IExternalServiceFactory), ServiceLifetime.Transient)]
    public void ShouldRegisterTransientService(Type serviceType, ServiceLifetime expectedLifetime)
    {
        // Arrange

        // Act
        var serviceDescriptor = _sut.FirstOrDefault(x => x.ServiceType == serviceType);

        // Assert
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor.Lifetime.Should().Be(expectedLifetime);
    }
    
    [Fact]
    public void AddApplication_ShouldNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        Action act = () => services.AddInfrastructure(configuration);

        // Assert
        act.Should().NotThrow();
    }
}