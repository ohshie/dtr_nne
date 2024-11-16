using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestAddNewsOutletService : BaseTestNewsOutletService
{
    public TestAddNewsOutletService()
    {
        ILogger<AddNewsOutletService> logger = new Mock<ILogger<AddNewsOutletService>>().Object;
        
        _sut = new AddNewsOutletService(logger: logger, 
            repository: Mockrepository.Object, 
            mapper: MockMapper.Object, unitOfWork: MockUnitOfWork.Object);
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
        Mockrepository
            .Setup
            (
                repository => repository.AddRange(It.IsAny<List<NewsOutlet>>()).Result
            )
            .Returns(true);
        MockMapper
            .Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(incomingNewsOutletDtos);
        
        // Act
        var newsOutlets = await _sut.AddNewsOutlets(incomingNewsOutletDtos);

        // Assert
        newsOutlets.Should().HaveCount(c => c == incomingNewsOutletDtos.Count);
        newsOutlets.Should().BeEquivalentTo(incomingNewsOutletDtos);
    }
}