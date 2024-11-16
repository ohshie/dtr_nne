using dtr_nne.Application.DTO.NewsOutlet;
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
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
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
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns([]);

        // Act
        var result = await Sut.Add(incomingNewsOutletDtos);
        
        // Assert
        result.Should().BeOfType<UnprocessableEntityResult>();
        var objectResult = (StatusCodeResult)result;
        objectResult.StatusCode.Should().Be(422);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsListOfDTOs(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        // Assemble
        MockAddNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletDtos).Result)
            .Returns(incomingNewsOutletDtos);

        // Act
        var result = await Sut.Add(incomingNewsOutletDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<List<NewsOutletDto>>();
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task Add_OnSuccess_ReturnsAddedOutletsDtos(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        // Assemble
        MockAddNewsOutletService
            .Setup(service => service.AddNewsOutlets(incomingNewsOutletsDtos).Result)
            .Returns(incomingNewsOutletsDtos);

        // Act
        var result = await Sut.Add(incomingNewsOutletsDtos);
        
        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeEquivalentTo(incomingNewsOutletsDtos);
    }
}