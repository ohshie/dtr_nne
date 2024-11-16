using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestAddNewsOutletService : BaseTestNewsOutletService
{
    private TestAddNewsOutletService()
    {
        ILogger<AddNewsOutletService> logger = new Mock<ILogger<AddNewsOutletService>>().Object;
        
        _sut = new AddNewsOutletService(logger: logger, 
            repository: MockNewsOutletRepository.Object, 
            mapper: Mapper, unitOfWork: MockUnitOfWork.Object);
    }

    private readonly AddNewsOutletService _sut;

    [Fact]
    public async Task AddNewsOutlets_WhenInvokedEmpty_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var newsOutlets = await _sut.AddNewsOutlets([]);

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c == 0);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task AddNewsOutlets_WhenInvokedWithProperList_ReturnsAddedNewsOutletDtos(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Arrange
        var mappedNewsOutlets = Mapper.NewsOutletDtosToNewsOutlets(incomingNewsOutletDtos);
        
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.AddRange(mappedNewsOutlets).Result
            )
            .Returns(true);
        
        // Act
        var newsOutlets = await _sut.AddNewsOutlets(incomingNewsOutletDtos);

        // Assert
        newsOutlets.Should().HaveCount(c => c == incomingNewsOutletDtos.Count);
        newsOutlets.Should().BeEquivalentTo(incomingNewsOutletDtos);
    }
}