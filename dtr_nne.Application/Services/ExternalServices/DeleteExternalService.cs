using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;

namespace dtr_nne.Application.Services.ExternalServices;

internal class DeleteExternalService(ILogger<DeleteExternalService> logger, 
    IExternalServiceManagerHelper helper) : IDeleteExternalService
{
    public async Task<ErrorOr<ExternalServiceDto>> Delete(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting delete method for service {Service}", serviceDto.ServiceName);
        var requiredService = helper.FindRequiredExistingService(serviceDto);
        if (requiredService.IsError)
        {
            return requiredService.FirstError;
        }
        
        var success = await helper.PerformDataOperation(requiredService.Value, "delete");
        if (success is { IsError: false, Value: true })
        {
            logger.LogInformation("Successfully removed {ServiceName} from database", requiredService.Value.ServiceName);
            return serviceDto;
        }
        
        logger.LogError("Failed to perform data operations on External service {Service}", requiredService.Value.ServiceName);
        return Errors.DbErrors.RemovingFailed;
    }
}

public interface IDeleteExternalService
{
    public Task<ErrorOr<ExternalServiceDto>> Delete(ExternalServiceDto serviceDto);
}