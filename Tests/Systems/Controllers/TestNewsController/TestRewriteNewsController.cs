using dtr_nne.Application.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Systems.Controllers.TestNewsController;

public class TestRewriteNewsController : BaseTestNewsController
{
    [Fact]
    public async Task RewriteNews_OnSuccess_Returns200()
    {
        // Arrange

        // Act
        var result = await Sut.RewriteNews(TestArticleContentDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RewriteNews_OnError_Returns400()
    {
        // Assemble
        MockNewsRewriter
            .Setup(rewriter => rewriter.Rewrite(TestArticleContentDto).Result)
            .Returns(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);

        // Act
        var result = await Sut.RewriteNews(TestArticleContentDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}