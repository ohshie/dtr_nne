using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers;

public class TestTranslatorApiController
{
    private readonly Mock<IExternalServiceManager> _mockTranslatorApiService;
    private readonly TranslatorApiController _sut;
    private readonly Mock<ExternalServiceDto> _mockService = new();

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
        var result = await _sut.Add(_mockService.Object);

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
            service => service.Add(_mockService.Object).Result).Returns(_mockService.Object);

        // Act
        await _sut.Add(_mockService.Object);

        // Assert 
        _mockTranslatorApiService.Verify(service => service.Add(_mockService.Object), Times.Once);
    }

    [Fact]
    public async Task Add_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.Add(_mockService.Object).Result).Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.Add(_mockService.Object);

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
        var result = await _sut.UpdateKey(_mockService.Object);

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
            service => service.Update(_mockService.Object).Result).Returns(_mockService.Object);

        // Act
        await _sut.UpdateKey(_mockService.Object);

        // Assert 
        _mockTranslatorApiService.Verify(service => service.Update(_mockService.Object), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockTranslatorApiService.Setup(
            service => service.Update(_mockService.Object).Result).Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.UpdateKey(_mockService.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}