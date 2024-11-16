using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestUpdateNewsOutletService : BaseTestNewsOutletService
{
    private readonly UpdateNewsOutletService _sut;

    private TestUpdateNewsOutletService()
    {
        ILogger<UpdateNewsOutletService> logger = new Mock<ILogger<UpdateNewsOutletService>>().Object;
        
        _sut = new UpdateNewsOutletService(logger: logger, 
            repository: MockNewsOutletRepository.Object, 
            mapper: Mapper, unitOfWork: MockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task UpdateNewsOutlets_WhenInvokedWithEmptyList_ReturnsEmptyList()
    {
        // Assemble

        // Act
        var result = await _sut.UpdateNewsOutlets([]);

        // Assert 
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task UpdateNewsOutlets_WhenInvokedWithProperList_ReturnsUpdatedList(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.UpdateRange(It.IsAny<List<NewsOutlet>>())
            )
            .Returns(true);
        
        // Act
        var result = await _sut.UpdateNewsOutlets(incomingNewsOutletDtos);

        // Assert 
        result.Should().BeEquivalentTo(incomingNewsOutletDtos);
    }
}