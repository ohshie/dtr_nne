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
        _mockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, _testService.ApiKey))
            .Returns(_mockLlmService.Object);

        // Act
        var result = await _sut.CheckKeyValidity(_testService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenAssistantRunError_ReturnsSpecificError()
    {
        // Arrange
        _mockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, _testService.ApiKey))
            .Returns(_mockLlmService.Object);
        
        _mockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Llm.AssistantRunError);
        
        // Act
        var result = await _sut.CheckKeyValidity(_testService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Llm.AssistantRunError);
    }
}