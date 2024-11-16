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
        MockApiKeyMapper = new Mock<IApiKeyMapper>();
        MockTranslatorRepository = new Mock<ITranslatorApiRepository>();
        MockApiKeyDto = new Mock<TranslatorApiDto>();
        MockApiKey = new Mock<TranslatorApi>();
        MockUnitOfWork = new Mock<IUnitOfWork<NneDbContext>>();

        ResetMockState();
        
        Sut = new(translatorService: MockTranslatorService.Object, 
            apiKeyMapper: MockApiKeyMapper.Object, 
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
            .Setup(mapper => mapper.MapTranslatorApiDtoToTranslatorApi(MockApiKeyDto.Object))
            .Returns(MockApiKey.Object);

        MockUnitOfWork
            .Setup(work => work.Save().Result)
            .Returns(true);
        
        MockTranslatorService
            .Setup(service => service.Translate(It.IsAny<List<Headline>>(), MockApiKey.Object).Result)
            .Returns(new ErrorOr<List<Headline>>());
        
        MockTranslatorRepository
            .Setup(repository => repository.Add(MockApiKey.Object).Result)
            .Returns(true);
        
        MockTranslatorRepository
            .Setup(repository => repository.Update(MockApiKey.Object))
            .Returns(true);
        
        MockTranslatorRepository
            .Setup(repository => repository.Get(1).Result)
            .Returns(MockApiKey.Object);
    }

    internal Mock<IUnitOfWork<NneDbContext>> MockUnitOfWork { get; }

    internal Mock<TranslatorApi> MockApiKey { get; }

    internal Mock<TranslatorApiDto> MockApiKeyDto { get; }

    internal Mock<ITranslatorService> MockTranslatorService { get; }
    internal Mock<IApiKeyMapper> MockApiKeyMapper { get; }
    internal Mock<ITranslatorApiRepository> MockTranslatorRepository { get; }
    internal TranslatorApiKeyService Sut { get; }
}