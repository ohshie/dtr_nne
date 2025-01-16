using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestDeleteNewsOutletController : BaseTestNewsOutletController
{
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Delete_OnSuccess_ReturnsEmptyListOfDeletedDto(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(new List<NewsOutletDto>{Capacity = 0});

        // Act
        var result = await Sut.Delete(incomingNewsOutletsDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Should().BeOfType<OkObjectResult>();
    }
    
    [Fact]
    public async Task Delete_OnInvoke_ShouldCallNewsOutletServiceDelete()
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(new List<NewsOutletDto>()).Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await Sut.Delete(new List<NewsOutletDto>());
        
        // Assert 
        MockDeleteNewsOutletService.Verify(service => service.DeleteNewsOutlets(new List<NewsOutletDto>()), Times.Once);
    }
    
    [Fact]
    public async Task Delete_WhenNoNewsOutletsInDb_ShouldReturnBadRequest()
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.DeleteNewsOutlets(It.IsAny<List<NewsOutletDto>>()).Result)
            .Returns(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await Sut.Delete([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}