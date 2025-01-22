using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers.TestNewsController;

public class TestParseArticleController : BaseTestNewsController
{
    [Fact]
    public async Task RewriteNews_OnSuccess_Returns200()
    {
        // Arrange

        // Act
        var result = await Sut.ParseArticle(It.IsAny<BaseNewsArticleDto>());

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
            .Setup(rewriter => rewriter.Execute(It.IsAny<NewsArticleDto>()).Result)
            .Returns(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));

        // Act
        var result = await Sut.ParseArticle(It.IsAny<NewsArticleDto>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}