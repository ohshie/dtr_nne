using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;

namespace dtr_nne.Application.Services.ExternalServices;

internal class UpdateExternalService(ILogger<UpdateExternalService> logger,
    IExternalServiceManagerHelper helper, 
    IExternalServiceMapper mapper) : IUpdateExternalService
{
    public async Task<ErrorOr<ExternalServiceDto>> Update(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Update method for service {Service}", serviceDto.ServiceName);
        var mappedServiceDto = mapper.DtoToService(serviceDto);
        
        var requiredServiceExist = helper.FindRequiredExistingService(mappedServiceDto);
        if (requiredServiceExist.IsError)
        {
            return requiredServiceExist.FirstError;
        }
        
        var serviceToUpdate = requiredServiceExist.Value;
        
        var mappedIncomingService = mapper.DtoToService(serviceDto);
        
        if (mappedIncomingService.ApiKey == serviceToUpdate.ApiKey)
        {
            var validKey = await helper.CheckKeyValidity(mappedIncomingService);
            if (validKey.IsError)
            {
                return validKey.FirstError;
            }
        }
        
        mappedIncomingService.Id = serviceToUpdate.Id;
        serviceToUpdate = mappedIncomingService;
        
        var success = await helper.PerformDataOperation(serviceToUpdate, "update");
        if (success is { IsError: false, Value: true })
        {
            logger.LogInformation("Successfully updated {ServiceName}", serviceToUpdate.ServiceName);
            var mappedProcessedServiceDto = mapper.ServiceToDto(serviceToUpdate);
            return mappedProcessedServiceDto;
        }
        
        logger.LogError("Failed to perform data operations on External service {Service}", serviceToUpdate.ServiceName);
        return Errors.DbErrors.UpdatingDbFailed;
    }
}

public interface IUpdateExternalService
{
    public Task<ErrorOr<ExternalServiceDto>> Update(ExternalServiceDto serviceDto);
}