using dtr_nne.Application.DTO;
using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers;

public class TestNewsOutletController
{
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsStatusCode200(List<NewsOutletDto> newsOutletDtos)
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();
        var sut = new NewsOutletController(newsOutletService.Object);
        
        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(newsOutletDtos);
        
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
            .Returns(new List<NewsOutletDto>());
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        await sut.Get();
        
        // Assert
        newsOutletService.Verify(
            service => service.GetAllNewsOutlets(), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsListOfUsers(List<NewsOutletDto> newsOutlets)
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();
        
        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(newsOutlets);
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        var result = await sut.Get();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }
    
    [Fact]
    public async Task Get_OnNoUsersFound_Returns404()
    {
        // Arrange 
        var newsOutletService = new Mock<INewsOutletService>();

        newsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(new List<NewsOutletDto>());
        
        var sut = new NewsOutletController(newsOutletService.Object);

        // Act
        var result = await sut.Get();
        
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var objectResult = (NotFoundObjectResult)result;
        objectResult.StatusCode.Should().Be(404);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_Returns201(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        var mockNewsOutletService = new Mock<INewsOutletService>();

        mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);

        var sut = new NewsOutletController(mockNewsOutletService.Object);
        
        // Act
        var result = await sut.Add(incomingNewsOutletDtos);
        
        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnFailure_ReturnsUnprocessableEntity(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        var mockNewsOutletService = new Mock<INewsOutletService>();

        mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns([]);

        var sut = new NewsOutletController(mockNewsOutletService.Object);
        
        // Act
        var result = await sut.Add(incomingNewsOutletDtos);
        
        // Assert
        result.Should().BeOfType<UnprocessableEntityResult>();
        var objectResult = (StatusCodeResult)result;
        objectResult.StatusCode.Should().Be(422);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsListOfDTOs(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        var mockNewsOutletService = new Mock<INewsOutletService>();

        mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);

        var sut = new NewsOutletController(mockNewsOutletService.Object);
        
        // Act
        var result = await sut.Add(incomingNewsOutletDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsAddedOutletsDtos(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        var mockNewsOutletService = new Mock<INewsOutletService>();

        mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        var sut = new NewsOutletController(mockNewsOutletService.Object);
        
        // Act
        var result = await sut.Add(incomingNewsOutletsDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeEquivalentTo(incomingNewsOutletsDtos);
    }
}