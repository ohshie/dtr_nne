using Bogus;
using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.ExternalServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerBase
{
    public TestExternalServiceManagerBase()
    {
        MockHelper = new();
        MockRepository = new();
        MockUow = new();
        MockMapper = new();
        MockServiceProvider = new();
        MockLlmService = new();
        
        var faker = new Faker();

        TestBaseExternalServiceDto = new()
        {
            Id = faker.Random.Int(0, 100),
            ServiceName = faker.Internet.UserName()
        };
        
        TestServiceDto = new()
        {
            ServiceName = TestBaseExternalServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };
        
        TestService = new()
        {
            Id = TestBaseExternalServiceDto.Id,
            ServiceName = TestServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = TestServiceDto.ApiKey,
            InUse = true
        };

        TestExistingService = new()
        {
            Id = TestBaseExternalServiceDto.Id,
            ServiceName = TestServiceDto.ServiceName,
            Type = ExternalServiceType.Llm,
            ApiKey = faker.Random.Guid().ToString(),
            InUse = true
        };
        
        DefaultSetup();
    }
    
    internal readonly Mock<IExternalServiceProviderRepository> MockRepository;
    internal readonly Mock<IUnitOfWork<INneDbContext>> MockUow;
    internal readonly Mock<IExternalServiceMapper> MockMapper;
    internal readonly Mock<IExternalServiceProvider> MockServiceProvider;
    internal readonly Mock<ILlmService> MockLlmService;
    internal readonly Mock<IExternalServiceManagerHelper> MockHelper;

    internal readonly BaseExternalServiceDto TestBaseExternalServiceDto;
    internal readonly ExternalServiceDto TestServiceDto;
    internal readonly ExternalService TestExistingService;
    internal readonly ExternalService TestService;

    private void DefaultSetup()
    {
        MockMapper
            .Setup(mapper => mapper.DtoToService(TestServiceDto))
            .Returns(TestService);

        MockMapper
            .Setup(mapper => mapper.ServiceToDto(TestService))
            .Returns(TestServiceDto);
        
        MockMapper
            .Setup(mapper => mapper.DtoToService(TestBaseExternalServiceDto))
            .Returns(TestService);

        MockHelper
            .Setup(helper => helper.CheckKeyValidity(TestService).Result)
            .Returns(true);

        MockHelper
            .Setup(helper => helper.PerformDataOperation(TestService, It.IsAny<string>()).Result)
            .Returns(true);

        MockHelper
            .Setup(helper => helper.FindRequiredExistingService(TestService))
            .Returns(TestService);
        
        MockLlmService
            .Setup(x => x.ProcessArticleAsync(It.IsAny<ArticleContent>()))
            .ReturnsAsync(It.IsAny<ArticleContent>());
        
        MockServiceProvider
            .Setup(x => x.Provide(TestService.Type, TestService.ApiKey))
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
}