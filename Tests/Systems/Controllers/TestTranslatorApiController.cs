using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.TranslatorServices;
using dtr_nne.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers;

public class TestTranslatorApiController
{
    private readonly Mock<ITranslatorApiKeyService> _mockTranslatorApiService;
    private readonly TranslatorApiController _sut;
    private readonly Mock<TranslatorApiDto> _mockApiKey = new();

    public TestTranslatorApiController()
    {
        _mockTranslatorApiService = new();
        _sut = new TranslatorApiController(_mockTranslatorApiService.Object);
    }


    [Fact]
    public async Task Add_OnSuccess_Returns201()
    {
        // Assemble

        // Act
        var result = await _sut.Add(_mockApiKey.Object);

        // Assert 
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Add_OnInvoke_ShouldCallTranslatorApiService()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.Add(_mockApiKey.Object).Result).Returns(_mockApiKey.Object);

        // Act
        await _sut.Add(_mockApiKey.Object);

        // Assert 
        _mockTranslatorApiService.Verify(service => service.Add(_mockApiKey.Object), Times.Once);
    }

    [Fact]
    public async Task Add_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.Add(_mockApiKey.Object).Result).Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.Add(_mockApiKey.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateKey_OnSuccess_ShouldReturn200()
    {
        // Assemble

        // Act
        var result = await _sut.UpdateKey(_mockApiKey.Object);

        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateKey_OnInvoke_ShouldCall_TranslatorApiService()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.UpdateKey(_mockApiKey.Object).Result).Returns(_mockApiKey.Object);

        // Act
        await _sut.UpdateKey(_mockApiKey.Object);

        // Assert 
        _mockTranslatorApiService.Verify(service => service.UpdateKey(_mockApiKey.Object), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.UpdateKey(_mockApiKey.Object).Result).Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.UpdateKey(_mockApiKey.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}