using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestGetNewsOutletController : BaseTestNewsOutletController
{
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsStatusCode200(List<NewsOutletDto> newsOutletDtos)
    {
        // Arrange 
        MockGetNewsOutletService
            .Setup(service => service.GetAll().Result)
            .Returns(newsOutletDtos);
        
        // Act
        var result = (OkObjectResult)await Sut.Get();
        
        // Assert
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_OnSuccess_InvokesNewsOutletService()
    {
        // Arrange 
        MockGetNewsOutletService
            .Setup(service => service.GetAll().Result)
            .Returns(new List<NewsOutletDto>());

        // Act
        await Sut.Get();
        
        // Assert
        MockGetNewsOutletService.Verify(
            service => service.GetAll(), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Get_OnSuccess_ReturnsListOfUsers(List<NewsOutletDto> newsOutlets)
    {
        // Arrange 
        MockGetNewsOutletService
            .Setup(service => service.GetAll().Result)
            .Returns(newsOutlets);
        
        // Act
        var result = await Sut.Get();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.Value.Should().BeEquivalentTo(newsOutlets);
    }

    [Fact]
    public async Task Get_OnNoNewsOtletsFound_ReturnsError()
    {
        // Arrange 
        MockGetNewsOutletService
            .Setup(service => service.GetAll().Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
       
        // Act
        var result = await Sut.Get();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.Value.Should().NotBeOfType<List<NewsOutlet>>();
    }
}