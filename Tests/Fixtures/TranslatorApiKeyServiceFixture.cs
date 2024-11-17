using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.DTO.Translator;
using dtr_nne.Application.ExternalServices.TranslatorServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Fixtures;

public class TranslatorApiKeyServiceFixture
{
    public TranslatorApiKeyServiceFixture()
    {
        MockTranslatorService = new Mock<ITranslatorService>();
        MockApiKeyMapper = new Mock<IExternalServiceMapper>();
        MockTranslatorRepository = new Mock<IExternalServiceProviderRepository>();
        MockExternalServiceDto = new Mock<ExternalServiceDto>();
        MockExternalService = new Mock<ExternalService>();
        MockUnitOfWork = new Mock<IUnitOfWork<NneDbContext>>();

        ResetMockState();
        
        Sut = new(translatorService: MockTranslatorService.Object, 
            mapper: MockApiKeyMapper.Object, 
            repository: MockTranslatorRepository.Object,
            unitOfWork: MockUnitOfWork.Object,
            logger: new Mock<ILogger<TranslatorApiKeyService>>().Object);
    }

    internal void ResetMockState()
    {
        MockApiKeyMapper.Reset();
        MockUnitOfWork.Reset();
        MockTranslatorService.Reset();
        MockTranslatorRepository.Reset();
        
        MockApiKeyMapper
            .Setup(mapper => mapper.DtoToService(MockExternalServiceDto.Object))
            .Returns(MockExternalService.Object);

        MockUnitOfWork
            .Setup(work => work.Save().Result)
            .Returns(true);
        
        MockTranslatorService
            .Setup(service => service.Translate(It.IsAny<List<Headline>>(), MockExternalService.Object).Result)
            .Returns(new ErrorOr<List<Headline>>());
        
        MockTranslatorRepository
            .Setup(repository => repository.Add(MockExternalService.Object).Result)
            .Returns(true);
        
        MockTranslatorRepository
            .Setup(repository => repository.Update(MockExternalService.Object))
            .Returns(true);
        
        MockTranslatorRepository
            .Setup(repository => repository.Get(1).Result)
            .Returns(MockExternalService.Object);
    }

    internal Mock<IUnitOfWork<NneDbContext>> MockUnitOfWork { get; }

    internal Mock<ExternalService> MockExternalService { get; }

    internal Mock<ExternalServiceDto> MockExternalServiceDto { get; }

    internal Mock<ITranslatorService> MockTranslatorService { get; }
    internal Mock<IExternalServiceMapper> MockApiKeyMapper { get; }
    internal Mock<IExternalServiceProviderRepository> MockTranslatorRepository { get; }
    internal TranslatorApiKeyService Sut { get; }
}