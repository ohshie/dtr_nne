using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestManagedEntityService;

public class TestAddManagedEntity : BaseTestManagedEntityService
{
    public TestAddManagedEntity()
    {
        var logger = new Mock<ILogger<AddManagedEntity<NewsOutlet, NewsOutletDto>>>().Object;
        
        _sut = new AddManagedEntity<NewsOutlet, NewsOutletDto>(logger: logger, 
            repository: Mockrepository.Object, 
            mapper: MockMapper.Object, unitOfWork: MockUnitOfWork.Object);
    }

    private readonly AddManagedEntity<NewsOutlet, NewsOutletDto> _sut;

    [Fact]
    public async Task AddManagedEntity_WhenInvokedEmpty_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var newsOutlets = await _sut.Add([]);

        // Assert
        newsOutlets.IsError.Should().BeTrue();
        newsOutlets.FirstError.Should().Be(Errors.ManagedEntities.NoEntitiesProvided);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task AddManagedEntity_WhenInvokedWithProperList_ReturnsAddedNewsOutletDtos(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Arrange
        Mockrepository
            .Setup
            (
                repository => repository.AddRange(It.IsAny<List<NewsOutlet>>()).Result
            )
            .Returns(true);
        MockMapper
            .Setup(mapper => mapper.EntityToDto<NewsOutlet, NewsOutletDto>(It.IsAny<List<NewsOutlet>>()))
            .Returns(incomingNewsOutletDtos);
        
        // Act
        var newsOutlets = await _sut.Add(incomingNewsOutletDtos);

        // Assert
        newsOutlets.Value.Should().HaveCount(c => c == incomingNewsOutletDtos.Count);
        newsOutlets.Value.Should().BeEquivalentTo(incomingNewsOutletDtos);
    }
}