using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Controllers;
using dtr_nne.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers;

public class TestNewsOutletController
{
    [Fact]
    public async Task Get_OnSuccess_ReturnsStatusCode200()
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();
        var sut = new NewsOutletController(newsOutletService.Object);
        
        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(NewsOutletFixture.GetTestNewsOutlet);
        
        // Act
        var result = (OkObjectResult)await sut.Get();
        
        // Assert
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_OnSuccess_InvokesNewsOutletService()
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();

        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(new List<NewsOutlet>());
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        var result = await sut.Get();
        
        // Assert
        newsOutletService.Verify(
            service => service.GetAllNewsOutlets(), 
            Times.Once);
    }

    [Fact]
    public async Task Get_OnSuccess_ReturnsListOfUsers()
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();
        
        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(NewsOutletFixture.GetTestNewsOutlet);
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        var result = await sut.Get();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutlet>>();
    }
    
    [Fact]
    public async Task Get_OnNoUsersFound_Returns404()
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();

        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(new List<NewsOutlet>());
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        var result = await sut.Get();
        
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var objectResult = (NotFoundObjectResult)result;
        objectResult.StatusCode.Should().Be(404);
    }
}