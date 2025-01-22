using Bogus;
using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerBase
{
    public TestExternalServiceManagerBase()
    {
        MockRepository = new();
        MockUow = new();
        MockMapper = new();
        MockServiceProvider = new();
        MockLlmService = new();
        MockTranslatorService = new();
        
        var faker = new Faker();
        
        TestServiceDto = new()
        {
            ServiceName = faker.Internet.UserName(),
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };

        TestExistingService = new()
        {
            ServiceName = TestServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };
        
        TestService = new()
        {
            ServiceName = TestServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = TestServiceDto.ApiKey,
            InUse = true
        };
        
        DefaultSetup();
        
        Sut = new(logger: new Mock<ILogger<ExternalServiceManager>>().Object, 
            repository: MockRepository.Object,
            unitOfWork: MockUow.Object,
            mapper: MockMapper.Object, 
            serviceProvider: MockServiceProvider.Object);
    }

    internal readonly ExternalServiceManager Sut;
    internal readonly Mock<IExternalServiceProviderRepository> MockRepository;
    internal readonly Mock<IUnitOfWork<INneDbContext>> MockUow;
    internal readonly Mock<IExternalServiceMapper> MockMapper;
    internal readonly Mock<IExternalServiceProvider> MockServiceProvider;
    internal readonly Mock<ILlmService> MockLlmService;
    internal readonly Mock<ITranslatorService> MockTranslatorService;

    internal readonly ExternalServiceDto TestServiceDto;
    internal readonly ExternalService TestExistingService;
    internal readonly ExternalService TestService;

    internal void DefaultSetup()
    {
        MockMapper
            .Setup(mapper => mapper.DtoToService(TestServiceDto))
            .Returns(TestService);
        
        MockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<ArticleContent>()))
            .ReturnsAsync(It.IsAny<ArticleContent>());
        
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, ""))
            .Returns(MockLlmService.Object);
        
        MockRepository
            .Setup(x => x.Add(It.IsAny<ExternalService>()))
            .ReturnsAsync(true);
        
        MockRepository
            .Setup(x => x.Update(It.IsAny<ExternalService>()))
            .Returns(true);
        
        MockRepository
            .Setup(x => x.GetByType(ExternalServiceType.Llm))
            .Returns(new List<ExternalService> { TestExistingService });
        
        MockUow
            .Setup(x => x.Save())
            .ReturnsAsync(true);
    }
    
    [Fact]
    public async Task Add_WhenValidService_ReturnsSuccess()
    {
        // Arrange
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        // Act
        var result = await Sut.Add(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }
    
    [Fact]
    public async Task Add_WhenKeyValidationFails_ReturnsError()
    {
        // Arrange
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        MockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<ArticleContent>()))
            .ReturnsAsync(Errors.ExternalServiceProvider.Service.BadApiKey);
        
        // Act
        var result = await Sut.Add(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.BadApiKey);
    }
    
    [Fact]
    public async Task Add_WhenRepositoryFail_ReturnsDbError()
    {
        // Assemble
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        MockRepository
            .Setup(repository => repository.Add(TestService).Result).
            Returns(false);

        // Act
        var result = await Sut.Add(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
    
    [Fact]
    public async Task Add_WhenUowFail_ReturnsDbError()
    {
        // Assemble
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        MockUow
            .Setup(uow => uow.Save().Result).
            Returns(false);

        // Act
        var result = await Sut.Add(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UnitOfWorkSaveFailed);
    }
    
    [Fact]
    public async Task UpdateKey_WhenValidService_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await Sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestServiceDto);
    }

    [Fact]
    public async Task UpdateKey_WhenCheckKeyFail_ReturnsError()
    {
        // Assemble
        TestExistingService.ApiKey = TestServiceDto.ApiKey;
        MockServiceProvider.Reset();
        
        // Act
        var result = await Sut.Update(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        MockServiceProvider
            .Verify(x => 
                x.Provide(ExternalServiceType.Llm, TestService.ApiKey), 
                Times.Once);
        MockRepository.Verify(repository => repository.Update(It.IsAny<ExternalService>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateKey_WhenServiceNotFound_ReturnsError()
    {
        // Arrange
        MockRepository.Setup(x => x.GetByType(ExternalServiceType.Llm))
            .Returns(new List<ExternalService>());

        // Act
        var result = await Sut.Update(TestServiceDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public async Task Update_WhenRepositoryFail_ReturnsDbError()
    {
        // Assemble
        MockRepository
            .Setup(repository => repository.Update(TestService)).
            Returns(false);

        // Act
        var result = await Sut.Update(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.AddingToDbFailed);
    }
    
    [Fact]
    public async Task Update_WhenUowFail_ReturnsDbError()
    {
        // Assemble
        MockServiceProvider
            .Setup(x => x.Provide(ExternalServiceType.Llm, TestService.ApiKey))
            .Returns(MockLlmService.Object);
        
        MockUow
            .Setup(uow => uow.Save().Result).
            Returns(false);

        // Act
        var result = await Sut.Update(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.DbErrors.UnitOfWorkSaveFailed);
    }

    [Fact]
    public async Task CheckKeyValidity_WhenNoServiceFound_ReturnsSpecificError()
    {
        // Assemble
        MockServiceProvider.Reset();
        
        // Act
        var result = await Sut.CheckKeyValidity(TestService);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
    
    [Fact]
    public void FindRequiredExistingService_WhenServiceExists_ReturnsService()
    {
        // Arrange

        // Act
        var result = Sut.FindRequiredExistingService(TestServiceDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(TestExistingService);
    }

    [Fact]
    public void FindRequiredExistingService_WhenNoServiceExists_ReturnsError()
    {
        // Assemble
        MockRepository.Reset();

        // Act
        var result = Sut.FindRequiredExistingService(TestServiceDto);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.ExternalServiceProvider.Service.NoSavedServiceFound);
    }
}