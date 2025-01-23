using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

namespace Tests.Systems.Controllers.TestOpenAiAssistantController;

public class TestGetOpenAiAssistantController : BaseTestOpenAiAssistantController
{
    
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Get_OnSuccess_ReturnsStatusCode200(List<OpenAiAssistantDto> newsOutletDtos)
    {
        // Arrange 
        MockGetOpenAiAssistantService
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
        MockGetOpenAiAssistantService
            .Setup(service => service.GetAll().Result)
            .Returns(new List<OpenAiAssistantDto>());

        // Act
        await Sut.Get();
        
        // Assert
        MockGetOpenAiAssistantService.Verify(
            service => service.GetAll(), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Get_OnSuccess_ReturnsListOfUsers(List<OpenAiAssistantDto> newsOutlets)
    {
        // Arrange 
        MockGetOpenAiAssistantService
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
    public async Task Get_OnNoUsersFound_Returns404()
    {
        // Arrange 
        MockGetOpenAiAssistantService
            .Setup(service => service.GetAll().Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(OpenAiAssistant)));
       
        // Act
        var result = await Sut.Get();
        
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var objectResult = (NotFoundObjectResult)result;
        objectResult.StatusCode.Should().Be(404);
    }
}