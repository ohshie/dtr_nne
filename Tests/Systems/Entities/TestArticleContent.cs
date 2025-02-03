using System.Text.Json;
using dtr_nne.Domain.Entities;

namespace Tests.Systems.Entities;

public class TestArticleContent
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private static readonly Bogus.Faker Faker = new();

    [Fact]
    public void Deserialize_StringInput_ToString()
    {
        // Arrange
        var json = """{"body": "test content"}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.Body.Should().Be("test content");
    }
    
    [Fact]
    public void Deserialize_StringArrayInput_ToStringList()
    {
        // Arrange
        var json = """{"copyright": ["test", "copyright"]}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.Copyright.Should().HaveCount(2)
            .And.Contain("test")
            .And.Contain("copyright");
    }
    
    [Fact]
    public void Deserialize_MultipleUris()
    {
        // Arrange
        var url1 = Faker.Internet.UrlWithPath();
        var url2 = Faker.Internet.UrlWithPath();
        var json = $$"""{"images": ["{{url1}}", "{{url2}}"]}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().HaveCount(2);
        result.Images[0].ToString().Should().Be(url1);
        result.Images[1].ToString().Should().Be(url2);
    }
}