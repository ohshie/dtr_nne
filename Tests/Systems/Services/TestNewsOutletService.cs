using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services;

public class TestNewsOutletService
    : IClassFixture<GenericLoggerFixture<NewsOutletService>>
{
    private readonly Mock<INewsOutletRepository> _mockNewsOutletRepository;
    private readonly INewsOutletMapper _mapper;
    private readonly NewsOutletService _sut;
    
    public TestNewsOutletService(GenericLoggerFixture<NewsOutletService> loggerFixture)
    {
        _mockNewsOutletRepository = new Mock<INewsOutletRepository>();
        Mock<IUnitOfWork<INneDbContext>> mockUnitOfWork = new();
        _mapper = new NewsOutletMapper();
        
        _sut = new NewsOutletService(logger: loggerFixture.Logger, 
            repository: _mockNewsOutletRepository.Object, 
            mapper: _mapper, 
            unitOfWork: mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task GetAllUsers_WhenInvokedEmpty_ReturnEmptyNewsOutletList()
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
    public async Task GetAllUsers_WhenInvokedPopulated_ReturnsNewsOutletList(List<NewsOutlet> incomingNewsOutletDtos)
    {
        // Arrange
        _mockNewsOutletRepository
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
        var mappedNewsOutlets = _mapper.NewsOutletDtosToNewsOutlets(incomingNewsOutletDtos);
        
        _mockNewsOutletRepository
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
        _mockNewsOutletRepository
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

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task DeleteNewsOutlets_IfAllDeleted_ReturnsListOfEntitiesWithSameIdsAndName(List<DeleteNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var mappedNewsOutlets = _mapper.DeleteNewsOutletDtosToNewsOutlet(incomingDeleteNewsOutletDtos);

        _mockNewsOutletRepository.Setup(repository => repository.GetAll().Result)
            .Returns(mappedNewsOutlets);
        
        _mockNewsOutletRepository
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
        List<DeleteNewsOutletsDto> incomingDeleteNewsoutletDtos)
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
    public async Task DeleteNewsOutlets_IfSomeNotDeleted_ReturnsListOfEntitiesWithSameIdsAndName(List<DeleteNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var toBetrimmedIncomingDeleteNewsOutletDtos = incomingDeleteNewsOutletDtos.ToList();
        toBetrimmedIncomingDeleteNewsOutletDtos.RemoveAt(toBetrimmedIncomingDeleteNewsOutletDtos.Count-1);
        
        var trimmedMappedNewsOutlets = _mapper.DeleteNewsOutletDtosToNewsOutlet(toBetrimmedIncomingDeleteNewsOutletDtos);

        _mockNewsOutletRepository.Setup(
                repository => repository.GetAll().Result)
            .Returns(trimmedMappedNewsOutlets);
        
        _mockNewsOutletRepository
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
    public async Task DeleteNewsOutlets_WhenInvokedWithProperList_ShouldCallRepositoryRemoveRangeOnce(List<DeleteNewsOutletsDto> incomingDeleteNewsOutletDtos)
    {
        // Assemble
        var mappedNewsOutlets = _mapper.DeleteNewsOutletDtosToNewsOutlet(incomingDeleteNewsOutletDtos);
        
        _mockNewsOutletRepository.Setup(
                repository => repository.GetAll().Result)
            .Returns(mappedNewsOutlets);
        
        _mockNewsOutletRepository
            .Setup
            (
                repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>())
            )
            .Returns(true);

        // Act
        await _sut.DeleteNewsOutlets(incomingDeleteNewsOutletDtos);

        // Assert 
        _mockNewsOutletRepository.Verify(repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>()), Times.Once);
    }
}