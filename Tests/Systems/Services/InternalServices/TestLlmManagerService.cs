using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.ExternalServices.LlmServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices;

public class TestLlmManagerService
{
    public TestLlmManagerService()
    {
        _mockRepository = new();
        _mockUow = new();
        _mockMapper = new();
        _mockServiceProvider = new();
        _mockLlmService = new();
        
        var faker = new Bogus.Faker();
        
        _testServiceDto = new()
        {
            ServiceName = Faker.Internet.UserName(),
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };

        _testExistingService = new()
        {
            ServiceName = _testServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };
        
        _testService = new()
        {
            ServiceName = _testServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = _testServiceDto.ApiKey,
            InUse = true
        };
        
        DefaultSetup();
        
        _sut = new(logger: new Mock<ILogger<LlmManagerService>>().Object, 
            repository: _mockRepository.Object,
            unitOfWork: _mockUow.Object,
            mapper: _mockMapper.Object, 
            serviceProvider: _mockServiceProvider.Object);
    }

    private readonly LlmManagerService _sut;
    private readonly Mock<IExternalServiceProviderRepository> _mockRepository;
    private readonly Mock<IUnitOfWork<INneDbContext>> _mockUow;
    private readonly Mock<IExternalServiceMapper> _mockMapper;
    private readonly Mock<IExternalServiceProvider> _mockServiceProvider;
    private readonly Mock<ILlmService> _mockLlmService;

    private readonly ExternalServiceDto _testServiceDto;
    private readonly ExternalService _testExistingService;
    private readonly ExternalService _testService;

    private void DefaultSetup()
    {
        _mockMapper
            .Setup(mapper => mapper.DtoToService(_testServiceDto))
            .Returns(_testService);
        
        _mockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()))
            .ReturnsAsync(It.IsAny<Article>());
        
        _mockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, ""))
            .Returns(_mockLlmService.Object);
        
        _mockRepository
            .Setup(x => x.Add(It.IsAny<ExternalService>()))
            .ReturnsAsync(true);
        
        _mockRepository
            .Setup(x => x.Update(It.IsAny<ExternalService>()))
            .Returns(true);
        
        _mockRepository
            .Setup(x => x.GetByType(ExternalServiceType.Llm))
            .Returns(new List<ExternalService> { _testExistingService });
        
        _mockUow
            .Setup(x => x.Save())
            .ReturnsAsync(true);
    }
    
    [Fact]
    public async Task Add_WhenValidService_ReturnsSuccess()
    {
        // Arrange
        
        // Act
        var result = await _sut.Add(_testServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testServiceDto);
    }
    
    [Fact]
    public async Task Add_WhenKeyValidationFails_ReturnsError()
    {
        // Arrange
        _mockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Service.BadApiKey);
        
        // Act
        var result = await _sut.Add(_testServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.BadApiKey);
    }
    
    [Fact]
    public async Task Add_WhenRepositoryFail_ReturnsDbError()
    {
        // Assemble
        _mockRepository
            .Setup(repository => repository.Add(_testService).Result).
            Returns(false);

        // Act
        var result = await _sut.Add(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
    
    [Fact]
    public async Task Add_WhenUowFail_ReturnsDbError()
    {
        // Assemble
        _mockUow
            .Setup(uow => uow.Save().Result).
            Returns(false);

        // Act
        var result = await _sut.Add(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UnitOfWorkSaveFailed);
    }
    
    [Fact]
    public async Task UpdateKey_WhenValidService_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await _sut.UpdateKey(_testServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testServiceDto);
    }

    [Fact]
    public async Task UpdateKey_WhenCheckKeyFail_ReturnsError()
    {
        // Assemble
        _testExistingService.ApiKey = _testServiceDto.ApiKey;
        _mockServiceProvider.Reset();
        
        // Act
        var result = await _sut.UpdateKey(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        _mockServiceProvider
            .Verify(x => 
                x.Provide(It.IsAny<ExternalServiceType>(), ""), 
                Times.Once);
        _mockRepository.Verify(repository => repository.Update(It.IsAny<ExternalService>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateKey_WhenServiceNotFound_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByType(ExternalServiceType.Llm))
            .Returns(new List<ExternalService>());

        // Act
        var result = await _sut.UpdateKey(_testServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public async Task Update_WhenRepositoryFail_ReturnsDbError()
    {
        // Assemble
        _mockRepository
            .Setup(repository => repository.Update(_testService)).
            Returns(false);

        // Act
        var result = await _sut.UpdateKey(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
    
    [Fact]
    public async Task Update_WhenUowFail_ReturnsDbError()
    {
        // Assemble
        _mockUow
            .Setup(uow => uow.Save().Result).
            Returns(false);

        // Act
        var result = await _sut.UpdateKey(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UnitOfWorkSaveFailed);
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenValidKey_ReturnsTrue()
    {
        // Arrange

        // Act
        var result = await _sut.CheckKeyValidity(_testService, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckKeyValidity_WhenAssistantRunError_ReturnsSpecificError()
    {
        // Arrange
        _mockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<Article>(), It.IsAny<string>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Llm.AssistantRunError);
        
        // Act
        var result = await _sut.CheckKeyValidity(_testService, true);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Llm.AssistantRunError);
    }

    [Fact]
    public async Task CheckKeyValidity_WhenNoLlmServiceFound_ReturnsSpecificError()
    {
        // Assemble
        _mockServiceProvider.Reset();
        
        // Act
        var result = await _sut.CheckKeyValidity(_testService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public void FindRequiredExistingService_WhenServiceExists_ReturnsService()
    {
        // Arrange

        // Act
        var result = _sut.FindRequiredExistingService(_testServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_testExistingService);
    }

    [Fact]
    public void FindRequiredExistingService_WhenNoServiceExists_ReturnsError()
    {
        // Assemble
        _mockRepository.Reset();

        // Act
        var result = _sut.FindRequiredExistingService(_testServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
}