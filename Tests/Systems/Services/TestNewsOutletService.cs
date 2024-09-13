using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services;

public class TestNewsOutletService
{
    [Fact]
    public async Task GetAllUsers_WhenInvoked_ReturnsNewsOutletList()
    {
        // Arrange
        var sut = new NewsOutletService();

        // Act
        var newsOutlets = await sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().BeOfType<List<NewsOutlet>>();
    }

    [Fact]
    public async Task GetAllUsers_WhenInvoked_ReturnsPopulatedNewsOutletList()
    {
        // Arrange
        var mockNewsOutletRepository = new Mock<IRepository<NewsOutlet>>();
        var sut = new NewsOutletService();

        // Act
        var newsOutlets = await sut.GetAllNewsOutlets();

        // Assert
        newsOutlets.Should().BeOfType<List<NewsOutlet>>();
    }
}