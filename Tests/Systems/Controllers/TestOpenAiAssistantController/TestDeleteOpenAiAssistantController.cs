using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

namespace Tests.Systems.Controllers.TestOpenAiAssistantController;

public class TestDeleteOpenAiAssistantController : BaseTestOpenAiAssistantController
{
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Delete_OnSuccess_ReturnsEmptyListOfDeletedDto(List<OpenAiAssistantDto> incomingOpenAiAssistantDtos)
    {
        // Assemble
        MockDeleteOpenAiAssistantService
            .Setup(service => service.Delete(incomingOpenAiAssistantDtos).Result)
            .Returns(new List<OpenAiAssistantDto>{Capacity = 0});

        // Act
        var result = await Sut.Delete(incomingOpenAiAssistantDtos);

        // Assert 
        var objectResult = (ObjectResult)result;
        objectResult.Should().BeOfType<OkObjectResult>();
    }
    
    [Fact]
    public async Task Delete_OnInvoke_ShouldCallNewsOutletServiceDelete()
    {
        // Assemble
        MockDeleteOpenAiAssistantService
            .Setup(service => service.Delete(new List<OpenAiAssistantDto>()).Result)
            .Returns(new List<OpenAiAssistantDto>());

        // Act
        await Sut.Delete([]);
        
        // Assert 
        MockDeleteOpenAiAssistantService.Verify(service => 
            service.Delete(new List<OpenAiAssistantDto>()), Times.Once);
    }
    
    [Fact]
    public async Task Delete_WhenNoNewsOutletsInDb_ShouldReturnBadRequest()
    {
        // Assemble
        MockDeleteOpenAiAssistantService
            .Setup(service => service.Delete(It.IsAny<List<OpenAiAssistantDto>>()).Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(OpenAiAssistantDto)));

        // Act
        var result = await Sut.Delete([]);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}