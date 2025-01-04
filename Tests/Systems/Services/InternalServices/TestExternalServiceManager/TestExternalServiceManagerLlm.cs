using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerLlm : TestExternalServiceManagerBase
{
    [Fact]
    public async Task CheckKeyValidity_WhenValidLlmKey_ReturnsTrue()
    {
        // Arrange
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);

        // Act
        var result = await Sut.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenAssistantRunError_ReturnsSpecificError()
    {
        // Arrange
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        MockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Llm.AssistantRunError);
        
        // Act
        var result = await Sut.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Llm.AssistantRunError);
    }
}