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

public class TestNewsOutletService(GenericLoggerFixture<NewsOutletService> loggerFixture) 
    : IClassFixture<GenericLoggerFixture<NewsOutletService>>
{
    [Fact]
    public async Task GetAllUsers_WhenInvokedEmpty_ReturnEmptyNewsOutletList()
    {
        // Arrange
        var mockNewsOutletRepository = new Mock<IRepository<NewsOutlet>>();
        var mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        var mapper = new NewsOutletMapper();
        var sut = new NewsOutletService(loggerFixture.Logger, 
            mockNewsOutletRepository.Object, 
            mapper, 
            mockUnitOfWork.Object);

        // Act
        var newsOutlets = await sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c == 0);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task GetAllUsers_WhenInvokedPopulated_ReturnsNewsOutletList(List<NewsOutlet> newsOutletDtos)
    {
        // Arrange
        var mockNewsOutletRepository = new Mock<IRepository<NewsOutlet>>();
        var mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        var mapper = new NewsOutletMapper();
        var sut = new NewsOutletService(loggerFixture.Logger, 
            mockNewsOutletRepository.Object, 
            mapper, 
            mockUnitOfWork.Object);

        mockNewsOutletRepository
            .Setup
                (
                    repository => repository.GetAll().Result
                )
            .Returns(newsOutletDtos);
        
        // Act
        var newsOutlets = await sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c > 0);
    }
    
    [Fact]
    public async Task AddNewsOutlets_WhenInvokedEmpty_ReturnsEmptyList()
    {
        // Arrange
        var mockNewsOutletRepository = new Mock<IRepository<NewsOutlet>>();
        var mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        var mapper = new NewsOutletMapper();
        var sut = new NewsOutletService(loggerFixture.Logger, 
            mockNewsOutletRepository.Object, 
            mapper, 
            mockUnitOfWork.Object);
        
        // Act
        var newsOutlets = await sut.AddNewsOutlets([]);

        // Assert
        newsOutlets.Should().NotBeNull();
        newsOutlets.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Should().HaveCount(c => c == 0);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task AddNewsOutlets_WhenInvokedWithProperList_ReturnsAddedNewsOutletDtos(List<NewsOutletDto> newsOutletDtos)
    {
        // Arrange
        var mockNewsOutletRepository = new Mock<IRepository<NewsOutlet>>();
        var mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();
        var mapper = new NewsOutletMapper();
        var sut = new NewsOutletService(loggerFixture.Logger, 
            mockNewsOutletRepository.Object, 
            mapper, 
            mockUnitOfWork.Object);
        
        var mappedNewsOutlets = mapper.NewsOutletDtosToNewsOutlets(newsOutletDtos);
        
        mockNewsOutletRepository
            .Setup
            (
                repository => repository.AddRange(mappedNewsOutlets).Result
            )
            .Returns(true);
        
        // Act
        var newsOutlets = await sut.AddNewsOutlets(newsOutletDtos);

        // Assert
        newsOutlets.Should().HaveCount(c => c == newsOutletDtos.Count);
        newsOutlets.Should().BeEquivalentTo(newsOutletDtos);
    }
}