using dtr_nne.Application.DTO;
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
}