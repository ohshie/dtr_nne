using Microsoft.AspNetCore.Mvc;

namespace Tests.Systems.Controllers.TestNewsController;

public class TestRewriteNewsController : BaseTestNewsController
{
    [Fact]
    public async Task RewriteNews_OnSuccess_Returns200()
    {
        // Arrange

        // Act
        var result = await Sut.RewriteNews(_testArticleDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (OkObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }
}