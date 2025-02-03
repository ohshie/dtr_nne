using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Controllers;
using dtr_nne.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Systems.Controllers;

public class TestExternalServiceController
{
    public TestExternalServiceController()
    {
        _mockGetExternalService = new();
        _mockDeleteExternalService = new();
        _mockUpdateExternalService = new();
        _mockAddExternalService = new();
        _sut = new(_mockGetExternalService.Object, _mockAddExternalService.Object, 
            _mockUpdateExternalService.Object, _mockDeleteExternalService.Object);
    }
    
    private readonly Mock<IGetExternalService> _mockGetExternalService;
    private readonly Mock<IDeleteExternalService> _mockDeleteExternalService;
    private readonly Mock<IUpdateExternalService> _mockUpdateExternalService;
    private readonly Mock<IAddExternalService> _mockAddExternalService;
    private readonly ExternalServiceController _sut;
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
        _mockAddExternalService.Setup(
            service => service.Add(_mockExternalServiceDto.Object).Result).Returns(_mockExternalServiceDto.Object);

        // Act
        await _sut.Add(_mockExternalServiceDto.Object);

        // Assert 
        _mockAddExternalService.Verify(service => service.Add(_mockExternalServiceDto.Object), Times.Once);
    }
    
    [Fact]
    public async Task Add_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockAddExternalService.Setup(
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
        var result = await _sut.Update(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateKey_OnInvoke_ShouldCall_TranslatorApiService()
    {
        // Assemble
        _mockUpdateExternalService.Setup(
            service => service.Update(_mockExternalServiceDto.Object).Result)
            .Returns(_mockExternalServiceDto.Object);

        // Act
        await _sut.Update(_mockExternalServiceDto.Object);

        // Assert 
        _mockUpdateExternalService
            .Verify(service => service.Update(_mockExternalServiceDto.Object), 
                Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateKey_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockUpdateExternalService.Setup(
            service => service.Update(_mockExternalServiceDto.Object).Result)
            .Returns(Errors.Translator.Api.BadApiKey);

        // Act
        var result = await _sut.Update(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Delete_OnSuccess_ShouldReturn200()
    {
        // Assemble
        _mockDeleteExternalService
            .Setup(delete => delete.Delete(It.IsAny<ExternalServiceDto>()).Result)
            .Returns(new ErrorOr<ExternalServiceDto>().Value);

        // Act
        var result = await _sut.Delete(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task Delete_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockDeleteExternalService.Setup(
                service => service.Delete(_mockExternalServiceDto.Object).Result)
            .Returns(Errors.ExternalServiceProvider.Service.BadApiKey);

        // Act
        var result = await _sut.Delete(_mockExternalServiceDto.Object);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
    
    [Fact]
    public async Task Get_OnSuccess_ShouldReturn200()
    {
        // Assemble
        _mockGetExternalService
            .Setup(delete => delete.GetAllByType(It.IsAny<ExternalServiceType>()))
            .ReturnsAsync(new ErrorOr<List<ExternalServiceDto>>());

        // Act
        var result = await _sut.GetAll(ExternalServiceType.Llm);

        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task Get_IfServiceReturnsError_ShouldReturn400()
    {
        // Assemble
        _mockGetExternalService.Setup(
                service => service.GetAllByType(It.IsAny<ExternalServiceType>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Service.BadApiKey);

        // Act
        var result = await _sut.GetAll(ExternalServiceType.Llm);

        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(400);
    }
}