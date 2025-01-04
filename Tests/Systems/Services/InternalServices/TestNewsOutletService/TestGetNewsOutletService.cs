using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletDtoFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestGetNewsOutletService : BaseTestNewsOutletService
{
    private readonly GetNewsOutletService _sut;

    public TestGetNewsOutletService()
    {
        ILogger<GetNewsOutletService> logger = new Mock<ILogger<GetNewsOutletService>>().Object;

        var randomNewsOutlet = NewsOutletDtoFixtureBase.OutletDtos[0];
        
        MockMapper
            .Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(randomNewsOutlet);
        
        _sut = new GetNewsOutletService(logger: logger, 
            repository: Mockrepository.Object, 
            mapper: MockMapper.Object);
    }
    
    [Fact]
    public async Task GetAllNewsOutlets_WhenInvokedEmpty_ReturnEmptyNewsOutletList()
    {
        // Arrange

        // Act
        var newsOutlets = await _sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c == 0);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task GetAllNewsOutlets_WhenInvokedPopulated_ReturnsNewsOutletList(List<NewsOutlet> incomingNewsOutlet)
    {
        // Arrange
        Mockrepository
            .Setup(repository => repository.GetAll().Result)
            .Returns(incomingNewsOutlet);
        
        // Act
        var newsOutlets = await _sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c > 0);
    }
}