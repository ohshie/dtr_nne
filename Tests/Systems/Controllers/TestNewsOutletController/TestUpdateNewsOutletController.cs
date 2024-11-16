using dtr_nne.Application.DTO.NewsOutlet;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestUpdateNewsOutletController : BaseTestNewsOutletController
{
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Put_OnSuccess_Returns200(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
       // Arrange
       MockUpdateNewsOutletService
           .Setup(service => service.UpdateNewsOutlets(incomingNewsOutletsDtos).Result)
           .Returns(incomingNewsOutletsDtos);
       // Act
       var result = await Sut.Update(incomingNewsOutletsDtos);

       // Assert
       var statusCode = (OkObjectResult)result;
       statusCode.StatusCode.Should().Be(200);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Put_OnSuccess_ReturnsListOfChangedDto(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockUpdateNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await Sut.Update(incomingNewsOutletsDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }

    [Fact]
    public async Task Put_OnInvoke_ShouldCallNewsOutletServiceUpdate()
    {
        // Assemble
        MockUpdateNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(new List<NewsOutletDto>()).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await Sut.Update(new List<NewsOutletDto>());
        
        // Assert 
        MockUpdateNewsOutletService.Verify(service => service.UpdateNewsOutlets(new List<NewsOutletDto>()), Times.Once);
    }

    [Fact]
    public async Task Put_WhenReturnedEmptyListFromService_ShouldReturn422()
    {
        // Assemble
        MockUpdateNewsOutletService
            .Setup(service => service.UpdateNewsOutlets(It.IsAny<List<NewsOutletDto>>()).Result)
            .Returns([]);

        // Act
        var result = await Sut.Update([]);

        // Assert 
        result.Should().BeOfType<UnprocessableEntityResult>();
        var objectResult = (StatusCodeResult)result;
        objectResult.StatusCode.Should().Be(422);
    }
}