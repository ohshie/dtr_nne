using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsEditor.NewsRewriter;
using dtr_nne.Application.Services.NewsOutletServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Systems.ServiceCollections;

public class TestApplicationServiceCollection
{
    public TestApplicationServiceCollection()
    {
        _sut = CreateServiceCollection();
    }

    private IServiceCollection _sut;
    
    private IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddApplication(configuration);
        return services;
    }
    
    [Theory]
    [InlineData(typeof(INewsOutletServiceHelper), typeof(NewsOutletServiceHelper))]
    [InlineData(typeof(IGetNewsOutletService), typeof(GetNewsOutletService))]
    [InlineData(typeof(IAddNewsOutletService), typeof(AddNewsOutletService))]
    [InlineData(typeof(IUpdateNewsOutletService), typeof(UpdateNewsOutletService))]
    [InlineData(typeof(IDeleteNewsOutletService), typeof(DeleteNewsOutletService))]
    [InlineData(typeof(INewsOutletMapper), typeof(NewsOutletMapper))]
    [InlineData(typeof(IExternalServiceMapper), typeof(ExternalServiceMapper))]
    [InlineData(typeof(IArticleMapper), typeof(ArticleMapper))]
    [InlineData(typeof(IExternalServiceManager), typeof(ExternalServiceManager))]
    [InlineData(typeof(INewsRewriter), typeof(NewsRewriter))]
    public void ShouldRegisterTransientService(Type serviceType, Type implementationType)
    {
        // Arrange

        // Act
        var serviceDescriptor = _sut.FirstOrDefault(x => x.ServiceType == serviceType);

        // Assert
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(implementationType);
        serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }
    
    [Fact]
    public void AddApplication_ShouldNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        Action act = () => services.AddApplication(configuration);

        // Assert
        act.Should().NotThrow();
    }
}