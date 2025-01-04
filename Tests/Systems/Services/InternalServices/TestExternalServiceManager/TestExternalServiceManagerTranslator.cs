using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerTranslator : TestExternalServiceManagerBase
{
    public TestExternalServiceManagerTranslator()
    {
        var faker = new Bogus.Faker();
        _testTranslatorService = new()
        {
            ServiceName = Enum.GetName(typeof(TranslatorServiceType), TranslatorServiceType.Deepl)!,
            Type = ExternalServiceType.Translator,
            ApiKey = _testServiceDto.ApiKey,
            InUse = true
        };
        
        _mockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Translator, _testTranslatorService.ApiKey))
            .Returns(_mockTranslatorService.Object);
    }
    
    private readonly ExternalService _testTranslatorService;
    
    [Fact]
    public async Task CheckKeyValidity_WhenValidLlmKey_ReturnsTrue()
    {
        // Arrange

        // Act
        var result = await _sut.CheckKeyValidity(_testTranslatorService);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenAssistantRunError_ReturnsSpecificError()
    {
        // Arrange
        _mockTranslatorService
            .Setup(x => x.Translate(It.IsAny<List<Headline>>()))
            .ReturnsAsync(Errors.Translator.Service.NoHeadlineProvided);
        
        // Act
        var result = await _sut.CheckKeyValidity(_testTranslatorService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Translator.Service.NoHeadlineProvided);
    }
}