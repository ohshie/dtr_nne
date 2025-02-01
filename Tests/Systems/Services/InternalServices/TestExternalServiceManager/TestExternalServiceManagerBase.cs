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
using NSubstitute;

namespace Tests.Systems.Services.InternalServices.TestExternalServiceManager;

public class TestExternalServiceManagerBase
{
    public TestExternalServiceManagerBase()
    {
        MockHelper = Substitute.For<IExternalServiceManagerHelper>();
        MockRepository = Substitute.For<IExternalServiceProviderRepository>();
        MockUow = Substitute.For<IUnitOfWork<INneDbContext>>();
        MockMapper = Substitute.For<IExternalServiceMapper>();
        MockServiceProvider = Substitute.For<IExternalServiceProvider>();
        MockLlmService = Substitute.For<ILlmService>();
        
        var faker = new Faker();

        TestBaseExternalServiceDto = new()
        {
            Id = faker.Random.Int(0, 100),
            Type = ExternalServiceType.Llm,
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

    internal readonly IExternalServiceProviderRepository MockRepository;
    internal readonly IUnitOfWork<INneDbContext> MockUow;
    internal readonly IExternalServiceMapper MockMapper;
    internal readonly IExternalServiceProvider MockServiceProvider;
    internal readonly ILlmService MockLlmService;
    internal readonly IExternalServiceManagerHelper MockHelper;

    internal readonly BaseExternalServiceDto TestBaseExternalServiceDto;
    internal readonly ExternalServiceDto TestServiceDto;
    internal readonly ExternalService TestExistingService;
    internal ExternalService TestService;

    private void DefaultSetup()
    {
        MockMapper
            .DtoToService(TestServiceDto)
            .Returns(TestService);

        MockMapper
            .ServiceToDto(TestService)
            .Returns(TestServiceDto);
        
        MockMapper
            .DtoToService(TestBaseExternalServiceDto)
            .Returns(TestService);

        MockHelper
            .CheckKeyValidity(TestService)
            .Returns(true);

        MockHelper
            .PerformDataOperation(TestService, Arg.Any<string>())
            .Returns(true);

        MockHelper
            .FindRequiredExistingService(TestService)
            .Returns(TestService);
        
        MockLlmService
            .ProcessArticleAsync(Arg.Any<ArticleContent>())
            .Returns(new ArticleContent());
        
        MockServiceProvider
            .Provide(TestService.Type, TestService.ApiKey)
            .Returns(MockLlmService);
        
        MockRepository
            .Add(Arg.Any<ExternalService>())
            .Returns(true);
        
        MockRepository
            .Update(Arg.Any<ExternalService>())
            .Returns(true);

        MockRepository
            .GetByType(ExternalServiceType.Llm)
            .Returns([TestExistingService]);
        
        MockUow
            .Save()
            .Returns(true);
    }
}