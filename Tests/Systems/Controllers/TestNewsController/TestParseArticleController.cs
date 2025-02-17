using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Tests.Systems.Controllers.TestNewsController;

public class TestParseArticleController : BaseTestNewsController
{
    [Fact]
    public async Task RewriteNews_OnSuccess_Returns200()
    {
        // Arrange

        // Act
        var result = await Sut.ParseArticle(TestArticleDto.Uri!);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task RewriteNews_OnError_Returns400()
    {
        // Assemble
        MockNewsParser
            .ExecuteParse(TestArticleDto.Uri!)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));

        // Act
        var result = await Sut.ParseArticle(TestArticleDto.Uri!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}