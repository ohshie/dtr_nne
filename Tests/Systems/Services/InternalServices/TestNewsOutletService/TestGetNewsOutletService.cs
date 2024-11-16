using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestGetNewsOutletService : BaseTestNewsOutletService
{
    private readonly GetNewsOutletService _sut;

    private TestGetNewsOutletService()
    {
        ILogger<GetNewsOutletService> logger = new Mock<ILogger<GetNewsOutletService>>().Object;
        
        _sut = new GetNewsOutletService(logger: logger, 
            repository: MockNewsOutletRepository.Object, 
            mapper: Mapper);
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
    public async Task GetAllNewsOutlets_WhenInvokedPopulated_ReturnsNewsOutletList(List<NewsOutlet> incomingNewsOutletDtos)
    {
        // Arrange
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.GetAll().Result
            )
            .Returns(incomingNewsOutletDtos);
        
        // Act
        var newsOutlets = await _sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c > 0);
    }
}