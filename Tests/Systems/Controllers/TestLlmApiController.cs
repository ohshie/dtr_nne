using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices.LlmServices;
using dtr_nne.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers;

public class TestLlmApiController
{
    public TestLlmApiController()
    {
        _mockLlmApiKeyService = new();
        _sut = new(_mockLlmApiKeyService.Object);
    }

    private readonly Mock<ILlmManagerService> _mockLlmApiKeyService;
    private readonly LlmApiController _sut;
    private readonly Mock<ExternalServiceDto> _mockExternalServiceDto = new();

    [Fact]
    public async Task Add_OnSuccess_Returns201()
    {
        // Assemble

        // Act
        var result = await _sut.Add(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<CreatedAtActionResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
    }
    
    [Fact]
    public async Task Add_OnInvoke_ShouldCallLlmApiService()
    {
        // Assemble
        _mockLlmApiKeyService.Setup(
            service => service.Add(_mockExternalServiceDto.Object).Result).Returns(_mockExternalServiceDto.Object);

        // Act
        await _sut.Add(_mockExternalServiceDto.Object);

        // Assert 
        _mockLlmApiKeyService.Verify(service => service.Add(_mockExternalServiceDto.Object), Times.Once);
    }
    
    [Fact]
    public async Task Add_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockLlmApiKeyService.Setup(
            service => service.Add(_mockExternalServiceDto.Object).Result).Returns(Errors.ExternalServiceProvider.Service.BadApiKey);

        // Act
        var result = await _sut.Add(_mockExternalServiceDto.Object);

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
        var result = await _sut.UpdateKey(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateKey_OnInvoke_ShouldCall_TranslatorApiService()
    {
        // Assemble
        _mockLlmApiKeyService.Setup(
            service => service.UpdateKey(_mockExternalServiceDto.Object).Result).Returns(_mockExternalServiceDto.Object);

        // Act
        await _sut.UpdateKey(_mockExternalServiceDto.Object);

        // Assert 
        _mockLlmApiKeyService.Verify(service => service.UpdateKey(_mockExternalServiceDto.Object), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockLlmApiKeyService.Setup(
            service => service.UpdateKey(_mockExternalServiceDto.Object).Result).Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.UpdateKey(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}