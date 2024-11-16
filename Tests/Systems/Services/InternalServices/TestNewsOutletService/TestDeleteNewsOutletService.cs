using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestDeleteNewsOutletService : BaseTestNewsOutletService
{
    private readonly DeleteNewsOutletService _sut;

    private TestDeleteNewsOutletService()
    {
        ILogger<DeleteNewsOutletService> logger = new Mock<ILogger<DeleteNewsOutletService>>().Object;
        
        _sut = new DeleteNewsOutletService(logger: logger, 
            repository: MockNewsOutletRepository.Object, 
            mapper: Mapper, unitOfWork: MockUnitOfWork.Object);
    }
    
    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task DeleteNewsOutlets_IfAllDeleted_ReturnsListOfEntitiesWithSameIdsAndName(List<BaseNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var mappedNewsOutlets = Mapper.DeleteNewsOutletDtosToNewsOutlet(incomingDeleteNewsOutletDtos);

        MockNewsOutletRepository.Setup(repository => repository.GetAll().Result)
            .Returns(mappedNewsOutlets);
        
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>())
                )
            .Returns(true);

        // Act
        var result = await _sut.DeleteNewsOutlets(incomingDeleteNewsOutletDtos);

        // Assert
        result.Value.Should().HaveCount(0);
    }

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task DeleteNewsOutlets_IfNothingIndb_ReturnsError(
        List<BaseNewsOutletsDto> incomingDeleteNewsoutletDtos)
    {
        // Assemble
        
        // Act
        var result = await _sut.DeleteNewsOutlets(incomingDeleteNewsoutletDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().BeEquivalentTo(Errors.NewsOutlets.NotFoundInDb.Code);
    }

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task DeleteNewsOutlets_IfSomeNotDeleted_ReturnsListOfEntitiesWithSameIdsAndName(List<BaseNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var toBetrimmedIncomingDeleteNewsOutletDtos = incomingDeleteNewsOutletDtos.ToList();
        toBetrimmedIncomingDeleteNewsOutletDtos.RemoveAt(toBetrimmedIncomingDeleteNewsOutletDtos.Count-1);
        
        var trimmedMappedNewsOutlets = Mapper.DeleteNewsOutletDtosToNewsOutlet(toBetrimmedIncomingDeleteNewsOutletDtos);

        MockNewsOutletRepository.Setup(
                repository => repository.GetAll().Result)
            .Returns(trimmedMappedNewsOutlets);
        
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>())
            )
            .Returns(true);

        // Act
        var result = await _sut.DeleteNewsOutlets(incomingDeleteNewsOutletDtos);

        // Assert 
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCount(1);
        result.Value[^1].Id.Should().Match(no => no == incomingDeleteNewsOutletDtos[incomingDeleteNewsOutletDtos.Count-1].Id);
    }

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task DeleteNewsOutlets_WhenInvokedWithProperList_ShouldCallRepositoryRemoveRangeOnce(List<BaseNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var mappedNewsOutlets = Mapper.DeleteNewsOutletDtosToNewsOutlet(incomingDeleteNewsOutletDtos);
        
        MockNewsOutletRepository.Setup(
                repository => repository.GetAll().Result)
            .Returns(mappedNewsOutlets);
        
        MockNewsOutletRepository
            .Setup
            (
                repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>())
            )
            .Returns(true);

        // Act
        await _sut.DeleteNewsOutlets(incomingDeleteNewsOutletDtos);

        // Assert 
        MockNewsOutletRepository.Verify(repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>()), Times.Once);
    }
}