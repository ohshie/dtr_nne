using System.Diagnostics.CodeAnalysis;
using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.ExternalServices.LlmServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.ExternalServices;

public class TestOpenAiService
{
    [Fact]
    [Experimental("OPENAI001")]
    public async Task METHOD()
    {
        // Assemble
        var mockLogger = new Mock<ILogger<OpenAiService>>();
        var sut = new OpenAiService(mockLogger.Object);
        var mockArticle = new Article();
        var mockKey = "";

        // Act
        var result = await sut.ProcessArticleAsync(mockArticle, mockKey);

        // Assert 

    }
}