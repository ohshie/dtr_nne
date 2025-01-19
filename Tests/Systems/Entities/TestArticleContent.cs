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
        result.Body.Should().Be("test content");
    }
    
    [Fact]
    public void Deserialize_StringArrayInput_ToString()
    {
        // Arrange
        var json = """{"body": ["test", "content"]}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result.Body.Should().Be("test content");
    }
    
    [Fact]
    public void Deserialize_StringInput_ToStringList()
    {
        // Arrange
        var json = """{"copyright": "test copyright"}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result.Copyright.Should().ContainSingle().And.Contain("test copyright");
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
        result.Copyright.Should().HaveCount(2)
            .And.Contain("test")
            .And.Contain("copyright");
    }
    
    [Fact]
    public void Serialize_StringValue()
    {
        // Arrange
        var content = new ArticleContent
        {
            Body = "test content",
            Source = "test source"
        };

        // Act
        var json = JsonSerializer.Serialize(content, Options);
        var result = JsonDocument.Parse(json);

        // Assert
        result.RootElement.GetProperty("body").GetString().Should().Be("test content");
        result.RootElement.GetProperty("source").GetString().Should().Be("test source");
    }
    
    [Fact]
    public void Serialize_StringListValue()
    {
        // Arrange
        var content = new ArticleContent
        {
            Copyright = ["test", "copyright"]
        };

        // Act
        var json = JsonSerializer.Serialize(content, Options);
        var result = JsonDocument.Parse(json);

        // Assert
        var copyright = result.RootElement.GetProperty("copyright");
        copyright.GetArrayLength().Should().Be(2);
        copyright[0].GetString().Should().Be("test");
        copyright[1].GetString().Should().Be("copyright");
    }
    
    [Fact]
    public void Deserialize_SingleUri()
    {
        // Arrange
        var url = Faker.Internet.UrlWithPath();
        var json = $$"""{"images": "{{url}}"}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle()
            .Which.ToString().Should().Be(url);
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
        result.Images.Should().HaveCount(2);
        result.Images[0].ToString().Should().Be(url1);
        result.Images[1].ToString().Should().Be(url2);
    }
    
    [Fact]
    public void Deserialize_InvalidUri_ReturnsEmptyList()
    {
        // Arrange
        var json = """{"images": "not a url"}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result.Images.Should().BeEmpty();
    }
    
    [Fact]
    public void Deserialize_MixedValidAndInvalidUris_ReturnsOnlyValidUris()
    {
        // Arrange
        var validUrl = Faker.Internet.UrlWithPath();
        var json = $$"""{"images": ["not a url", "{{validUrl}}", "also not a url"]}""";

        // Act
        var result = JsonSerializer.Deserialize<ArticleContent>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle()
            .Which.ToString().Should().Be(validUrl);
    }

    [Fact]
    public void Serialize_UriList()
    {
        // Arrange
        var url1 = new Uri(Faker.Internet.UrlWithPath());
        var url2 = new Uri(Faker.Internet.UrlWithPath());
        var content = new ArticleContent
        {
            Images = [url1, url2]
        };

        // Act
        var json = JsonSerializer.Serialize(content, Options);
        var result = JsonDocument.Parse(json);

        // Assert
        var images = result.RootElement.GetProperty("images");
        images.GetArrayLength().Should().Be(2);
        images[0].GetString().Should().Be(url1.ToString());
        images[1].GetString().Should().Be(url2.ToString());
    }

    [Fact]
    public void Serialize_EmptyUriList()
    {
        // Arrange
        var content = new ArticleContent();

        // Act
        var json = JsonSerializer.Serialize(content, Options);
        var result = JsonDocument.Parse(json);

        // Assert
        var images = result.RootElement.GetProperty("images");
        images.GetArrayLength().Should().Be(0);
    }
}