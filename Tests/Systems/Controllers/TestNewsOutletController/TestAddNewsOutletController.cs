using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Tests.Fixtures;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public class TestAddNewsOutletController : BaseTestNewsOutletController
{
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_Returns201(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        MockAddNewsOutletService
            .Setup(service => service.Add(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);
        
        // Act
        var result = await Sut.Add(incomingNewsOutletDtos);
        
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
        MockAddNewsOutletService
            .Setup(service => service.Add(incomingNewsOutletDtos).Result)
            .Returns(Errors.ManagedEntities.NoEntitiesProvided);

        // Act
        var result = await Sut.Add(incomingNewsOutletDtos);
        
        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(422);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsListOfDTOs(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        MockAddNewsOutletService
            .Setup(service => service.Add(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);

        // Act
        var result = await Sut.Add(incomingNewsOutletDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        var actualResult = objectResult.Value is ErrorOr<List<NewsOutletDto>> or ? or : default;
        actualResult.Value.Should().BeEquivalentTo(incomingNewsOutletDtos);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsAddedOutletsDtos(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockAddNewsOutletService
            .Setup(service => service.Add(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await Sut.Add(incomingNewsOutletsDtos);
        
        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        var actualResult = objectResult.Value is ErrorOr<List<NewsOutletDto>> or ? or : default;
        actualResult.Value.Should().BeEquivalentTo(incomingNewsOutletsDtos);
    }
}