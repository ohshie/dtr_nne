using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Application.ExternalServices.LlmServices;

public class LlmManagerService(
    ILogger<LlmManagerService> logger,
    IExternalServiceProviderRepository repository,
    IUnitOfWork<INneDbContext> unitOfWork,
    IExternalServiceMapper mapper,
    IExternalServiceProvider serviceProvider) : ILlmManagerService
{
    public async Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Add method for service: {Service}", serviceDto.ServiceName);

        var mappedIncomingService = mapper.DtoToService(serviceDto);
        
        var validService = await CheckKeyValidity(mappedIncomingService, newService: true);
        if (validService.IsError)
        {
            return validService.FirstError;
        }
        
        var success = await repository.Add(mappedIncomingService);
        if (!success)
        {
            logger.LogError("Failed to add External service to repository");
            return Errors.DbErrors.AddingToDbFailed;
        }

        success = await unitOfWork.Save();
        if (!success)
        {
            logger.LogError("Failed to save API key in unit of work");
            return Errors.DbErrors.UnitOfWorkSaveFailed;
        }
        
        return serviceDto;
    }

    public async Task<ErrorOr<ExternalServiceDto>> UpdateKey(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Update method for service {Service}", serviceDto.ServiceName);
        var requiredServiceExist = FindRequiredExistingService(serviceDto);
        if (requiredServiceExist.IsError)
        {
            return requiredServiceExist.FirstError;
        }

        var serviceToUpdate = requiredServiceExist.Value;

        var mappedIncomingService = mapper.DtoToService(serviceDto);

        if (ServiceKeyUpdateRequired(mappedIncomingService, serviceToUpdate))
        {
            var validKey = await CheckKeyValidity(mappedIncomingService);
            if (validKey.IsError)
            {
                return validKey.FirstError;
            }
        }
        
        mappedIncomingService.Id = serviceToUpdate.Id;
        serviceToUpdate = mappedIncomingService;
        
        var success = repository.Update(serviceToUpdate);
        if (!success)
        {
            logger.LogError("Failed to update External service in repository");
            return Errors.DbErrors.AddingToDbFailed;
        }
        
        success = await unitOfWork.Save();
        if (!success)
        {
            logger.LogError("Failed to save API key in unit of work");
            return Errors.DbErrors.UnitOfWorkSaveFailed;
        }

        return serviceDto;
    }

    internal ErrorOr<ExternalService> FindRequiredExistingService(ExternalServiceDto serviceDto)
    {
        if (repository.GetByType(serviceDto.Type) is not { } currentServices)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        if (currentServices.Find(s => s.ServiceName == serviceDto.ServiceName) is not { } serviceToUpdate)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        return serviceToUpdate;
    }

    internal bool ServiceKeyUpdateRequired(ExternalService incomingService, ExternalService existingService)
    {
        return incomingService.ApiKey == existingService.ApiKey;
    }

    internal async Task<ErrorOr<bool>> CheckKeyValidity(ExternalService incomingService, bool newService = false)
    {
        var llmService = await GetLlmService(incomingService.Type, newService);
        if (llmService is null)
        {
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }

        var success = await CheckApiKey(llmService, incomingService.ApiKey);
        if (success.IsError)
        {
            return success.FirstError;
        }

        return success.Value;
    }
    
    internal async Task<ILlmService?> GetLlmService(ExternalServiceType type, bool newService = false)
    {
        ILlmService? llmService;
        if (newService)
        {
            llmService = await serviceProvider.ProvideService(type) as ILlmService;
        }
        else
        {
            llmService = await serviceProvider.GetExistingInUseService(type) as ILlmService;
        }

        return llmService;
    }
    
    internal async Task<ErrorOr<bool>> CheckApiKey(ILlmService llmService, string incomingApiKey)
    {
        var testArticle = new Article { OriginalBody = "test" };
        var validKey = await llmService.ProcessArticleAsync(testArticle, incomingApiKey);
        if (validKey.IsError)
        {
            return validKey.FirstError == Errors.ExternalServiceProvider.Llm.AssistantRunError 
                ? Errors.ExternalServiceProvider.Llm.AssistantRunError 
                : Errors.ExternalServiceProvider.Service.BadApiKey;
        }
        
        return true;
    }
}