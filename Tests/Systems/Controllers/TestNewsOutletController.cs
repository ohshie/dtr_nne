using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Controllers;
using dtr_nne.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers;

public class TestNewsOutletController
{
    [Fact]
    public async Task Get_OnSuccess_ReturnsStatusCode200()
    {
        // Arrange 
        var sut = new NewsOutletController();

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
        var result = (OkObjectResult)await sut.Get();
        
        // Assert
    }
}