using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

namespace Tests.Systems.Controllers.TestOpenAiAssistantController;

public class TestUpdateOpenAiAssistantController : BaseTestOpenAiAssistantController
{
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Put_OnSuccess_Returns200(List<OpenAiAssistantDto> incomingNewsOutletsDtos)
    {
       // Arrange
       MockUpdateOpenAiAssistantService
           .Setup(service => service.Update(incomingNewsOutletsDtos).Result)
           .Returns(new List<OpenAiAssistantDto>());
       // Act
       var result = await Sut.Update(incomingNewsOutletsDtos);

       // Assert
       var statusCode = (OkResult)result;
       statusCode.StatusCode.Should().Be(200);
    }

    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Put_OnSuccess_ReturnsListOfChangedDto(List<OpenAiAssistantDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockUpdateOpenAiAssistantService
            .Setup(service => service.Update(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await Sut.Update(incomingNewsOutletsDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<OpenAiAssistantDto>>();
    }

    [Fact]
    public async Task Put_OnInvoke_ShouldCallNewsOutletServiceUpdate()
    {
        // Assemble
        MockUpdateOpenAiAssistantService
            .Setup(service => service.Update(new List<OpenAiAssistantDto>()).Result)
            .Returns(new List<OpenAiAssistantDto>());

        // Act
        await Sut.Update(new List<OpenAiAssistantDto>());
        
        // Assert 
        MockUpdateOpenAiAssistantService.Verify(service => service.Update(new List<OpenAiAssistantDto>()), Times.Once);
    }

    [Fact]
    public async Task Put_WhenReturnedEmptyListFromService_ShouldReturn422()
    {
        // Assemble
        MockUpdateOpenAiAssistantService
            .Setup(service => service.Update(It.IsAny<List<OpenAiAssistantDto>>()).Result)
            .Returns(Errors.ManagedEntities.NoEntitiesProvided);

        // Act
        var result = await Sut.Update([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}