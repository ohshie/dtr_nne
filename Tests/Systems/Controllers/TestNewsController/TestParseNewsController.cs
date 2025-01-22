using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Systems.Controllers.TestNewsController;

public class TestParseNewsController : BaseTestNewsController
{
    [Fact]
    public async Task RewriteNews_OnSuccess_Returns200()
    {
        // Arrange

        // Act
        var result = await Sut.ParseNews();

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
            .Setup(rewriter => rewriter.ExecuteBatchParse(true, "").Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));

        // Act
        var result = await Sut.ParseNews();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
    
    [Fact]
    public async Task RewriteNews_WhenNoNewNews_Returns206()
    {
        // Assemble
        MockNewsParser
            .Setup(rewriter => rewriter.ExecuteBatchParse(true, "").Result)
            .Returns(new List<NewsArticleDto>(){Capacity = 1});

        // Act
        var result = await Sut.ParseNews();

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var objectResult = (NoContentResult)result;
        objectResult.StatusCode.Should().Be(204);
    }
}