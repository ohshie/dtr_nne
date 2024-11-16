using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestDeleteNewsOutletController : BaseTestNewsOutletController
{
    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task Delete_WhenInvokedWithProperList_ShouldReturn200(List<BaseNewsOutletsDto> incomingNewsOutletDtos)
    {
        // Assemble
        MockDeleteNewsOutletService
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
        var result = await Sut.Delete(incomingNewsOutletDtos);

        // Assert 
        var statusCode = (OkObjectResult)result;
        statusCode.StatusCode.Should().Be(200);
    }
    
    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public async Task Delete_OnSuccess_ReturnsEmptyListOfDeletedDto(List<BaseNewsOutletsDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        var result = await Sut.Delete(incomingNewsOutletsDtos);

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
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(new List<BaseNewsOutletsDto>()).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await Sut.Delete(new List<BaseNewsOutletsDto>());
        
        // Assert 
        MockDeleteNewsOutletService.Verify(service => service.DeleteNewsOutlets(new List<BaseNewsOutletsDto>()), Times.Once);
    }
    
    [Fact]
    public async Task Delete_WhenNoNewsOutletsInDb_ShouldReturnBadRequest()
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(It.IsAny<List<BaseNewsOutletsDto>>()).Result)
            .Returns(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await Sut.Delete([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}