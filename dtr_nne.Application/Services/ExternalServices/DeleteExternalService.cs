using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;

namespace dtr_nne.Application.Services.ExternalServices;

internal class DeleteExternalService(ILogger<DeleteExternalService> logger, 
    IExternalServiceMapper mapper,
    IExternalServiceManagerHelper helper) : IDeleteExternalService
{
    public async Task<ErrorOr<ExternalServiceDto>> Delete(BaseExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting delete method for service {Service}", serviceDto.ServiceName);
        var mappedService = mapper.DtoToService(serviceDto);
        
        var requiredService = await helper.FindRequiredExistingService(mappedService);
        if (requiredService.IsError)
        {
            return requiredService.FirstError;
        }
        
        var success = await helper.PerformDataOperation(requiredService.Value, "delete");
        if (success is { IsError: false, Value: true })
        {
            var processedServiceDto = mapper.ServiceToDto(requiredService.Value);
            logger.LogInformation("Successfully removed {ServiceName} from database", requiredService.Value.ServiceName);
            return processedServiceDto;
        }
        
        logger.LogError("Failed to perform data operations on External service {Service}", requiredService.Value.ServiceName);
        return Errors.DbErrors.RemovingFailed;
    }
}

public interface IDeleteExternalService
{
    public Task<ErrorOr<ExternalServiceDto>> Delete(BaseExternalServiceDto serviceDto);
}