using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.NewsOutletServices;
using dtr_nne.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers;

public class TestNewsOutletController
{
    private readonly Mock<INewsOutletService> _mockNewsOutletService;
    
    private readonly NewsOutletController _sut;

    public TestNewsOutletController()
    {
        _mockNewsOutletService = new();
        _sut = new NewsOutletController(_mockNewsOutletService.Object);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsStatusCode200(List<NewsOutletDto> newsOutletDtos)
    {
        // Arrange 
        _mockNewsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(newsOutletDtos);
        
        // Act
        var result = (OkObjectResult)await _sut.Get();
        
        // Assert
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_OnSuccess_InvokesNewsOutletService()
    {
        // Arrange 
        _mockNewsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await _sut.Get();
        
        // Assert
        _mockNewsOutletService.Verify(
            service => service.GetAllNewsOutlets(), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsListOfUsers(List<NewsOutletDto> newsOutlets)
    {
        // Arrange 
        _mockNewsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(newsOutlets);
        
        // Act
        var result = await _sut.Get();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }
    
    [Fact]
    public async Task Get_OnNoUsersFound_Returns404()
    {
        // Arrange 
        _mockNewsOutletService
            .Setup(service => service.GetAllNewsOutlets().Result)
            .Returns(new List<NewsOutletDto>());
       
        // Act
        var result = await _sut.Get();
        
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
        _mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);
        
        // Act
        var result = await _sut.Add(incomingNewsOutletDtos);
        
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
        _mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns([]);

        // Act
        var result = await _sut.Add(incomingNewsOutletDtos);
        
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
        _mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);

        // Act
        var result = await _sut.Add(incomingNewsOutletDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsAddedOutletsDtos(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await _sut.Add(incomingNewsOutletsDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeEquivalentTo(incomingNewsOutletsDtos);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Put_OnSuccess_Returns200(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
       // Arrange
       _mockNewsOutletService
           .Setup(service => service.UpdateNewsOutlets(incomingNewsOutletsDtos).Result)
           .Returns(incomingNewsOutletsDtos);
       // Act
       var result = await _sut.Update(incomingNewsOutletsDtos);

       // Assert
       var statusCode = (OkObjectResult)result;
       statusCode.StatusCode.Should().Be(200);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Put_OnSuccess_ReturnsListOfChangedDto(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await _sut.Update(incomingNewsOutletsDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }

    [Fact]
    public async Task Put_OnInvoke_ShouldCallNewsOutletServiceUpdate()
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(new List<NewsOutletDto>()).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await _sut.Update(new List<NewsOutletDto>());
        
        // Assert 
        _mockNewsOutletService.Verify(service => service.UpdateNewsOutlets(new List<NewsOutletDto>()), Times.Once);
    }

    [Fact]
    public async Task Put_WhenReturnedEmptyListFromService_ShouldReturn422()
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(It.IsAny<List<NewsOutletDto>>()).Result)
            .Returns([]);

        // Act
        var result = await _sut.Update([]);

        // Assert 
        result.Should().BeOfType<UnprocessableEntityResult>();
        var objectResult = (StatusCodeResult)result;
        objectResult.StatusCode.Should().Be(422);
    }

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task Delete_WhenInvokedWithProperList_ShouldReturn200(List<DeleteNewsOutletsDto> incomingNewsOutletDtos)
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(new List<NewsOutletDto>
            {
                new NewsOutletDto
                {
                    InUse = false,
                    AlwaysJs = false,
                    Name = "null",
                    Website = null,
                    MainPagePassword = "null",
                    NewsPassword = "null"
                }
            });
        
        // Act
        var result = await _sut.Delete(incomingNewsOutletDtos);

        // Assert 
        var statusCode = (OkObjectResult)result;
        statusCode.StatusCode.Should().Be(200);
    }
    
    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task Delete_OnSuccess_ReturnsEmptyListOfDeletedDto(List<DeleteNewsOutletsDto> incomingNewsOutletsDtos)
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        var result = await _sut.Delete(incomingNewsOutletsDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Should().BeOfType<OkObjectResult>();
        var returnedList = objectResult.Value as List<NewsOutletDto>;
        returnedList.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Delete_OnInvoke_ShouldCallNewsOutletServiceDelete()
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(new List<DeleteNewsOutletsDto>()).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await _sut.Delete(new List<DeleteNewsOutletsDto>());
        
        // Assert 
        _mockNewsOutletService.Verify(service => service.DeleteNewsOutlets(new List<DeleteNewsOutletsDto>()), Times.Once);
    }
    
    [Fact]
    public async Task Delete_WhenNoNewsOutletsInDb_ShouldReturnBadRequest()
    {
        // Assemble
        _mockNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(It.IsAny<List<DeleteNewsOutletsDto>>()).Result)
            .Returns(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await _sut.Delete([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}