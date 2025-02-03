using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestDeleteNewsOutletController : BaseTestNewsOutletController
{
    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task Delete_OnSuccess_ReturnsEmptyListOfDeletedDto(List<BaseNewsOutletsDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.Delete(incomingNewsOutletsDtos).Result)
            .Returns(new List<BaseNewsOutletsDto>{Capacity = 0});

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
            .Setup(service => service.Delete(new List<BaseNewsOutletsDto>()).Result)
            .Returns(new List<BaseNewsOutletsDto>());

        // Act
        await Sut.Delete(new List<BaseNewsOutletsDto>());
        
        // Assert 
        MockDeleteNewsOutletService.Verify(service => service.Delete(new List<BaseNewsOutletsDto>()), Times.Once);
    }
    
    [Fact]
    public async Task Delete_WhenNoNewsOutletsInDb_ShouldReturnBadRequest()
    {
        // Assemble
        MockDeleteNewsOutletService
            .Setup(service => service.Delete(It.IsAny<List<BaseNewsOutletsDto>>()).Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));

        // Act
        var result = await Sut.Delete([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}