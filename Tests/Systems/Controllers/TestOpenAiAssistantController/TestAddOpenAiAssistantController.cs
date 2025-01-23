using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

namespace Tests.Systems.Controllers.TestOpenAiAssistantController;

public class TestAddOpenAiAssistantController : BaseTestOpenAiAssistantController
{
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Add_OnSuccess_Returns201(List<OpenAiAssistantDto> incomingOpenAiAssistantDtos)
    {
        // Assemble
        MockAddOpenAiAssistantService
            .Setup(service => service.Add(incomingOpenAiAssistantDtos).Result)
            .Returns(incomingOpenAiAssistantDtos);
        
        // Act
        var result = await Sut.Add(incomingOpenAiAssistantDtos);
        
        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
    }
    
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Add_OnFailure_ReturnsUnprocessableEntity(List<OpenAiAssistantDto> incomingOpenAiAssistantDtos)
    {
        // Assemble
        MockAddOpenAiAssistantService
            .Setup(service => service.Add(incomingOpenAiAssistantDtos).Result)
            .Returns(Errors.ManagedEntities.NoEntitiesProvided);

        // Act
        var result = await Sut.Add(incomingOpenAiAssistantDtos);
        
        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(422);
    }

    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Add_OnSuccess_ReturnsListOfDTOs(List<OpenAiAssistantDto> incomingOpenAiAssistantDtos)
    {
        // Assemble
        MockAddOpenAiAssistantService
            .Setup(service => service.Add(incomingOpenAiAssistantDtos).Result)
            .Returns(incomingOpenAiAssistantDtos);

        // Act
        var result = await Sut.Add(incomingOpenAiAssistantDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        var actualResult = objectResult.Value is ErrorOr<List<OpenAiAssistantDto>> or ? or : default;
        actualResult.Value.Should().BeEquivalentTo(incomingOpenAiAssistantDtos);
    }
    
    [Theory]
    [ClassData(typeof(OpenAiAssistantDtoFixture))]
    public async Task Add_OnSuccess_ReturnsAddedOutletsDtos(List<OpenAiAssistantDto> incomingOpenAiAssistantDtos)
    {
        // Assemble
        MockAddOpenAiAssistantService
            .Setup(service => service.Add(incomingOpenAiAssistantDtos).Result)
            .Returns(incomingOpenAiAssistantDtos);

        // Act
        var result = await Sut.Add(incomingOpenAiAssistantDtos);
        
        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        var actualResult = objectResult.Value is ErrorOr<List<OpenAiAssistantDto>> or ? or : default;
        actualResult.Value.Should().BeEquivalentTo(incomingOpenAiAssistantDtos);
    }
}