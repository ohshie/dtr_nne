using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;

namespace dtr_nne.Application.Services.ExternalServices;

internal class AddExternalService(ILogger<AddExternalService> logger,
    IExternalServiceMapper mapper,
    IExternalServiceManagerHelper helper) : IAddExternalService
{
    public async Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto)
    {
        logger.LogInformation("Starting Add method for service: {Service}", serviceDto.ServiceName);

        var mappedIncomingService = mapper.DtoToService(serviceDto);
        
        var validService = await helper.CheckKeyValidity(mappedIncomingService);
        if (validService.IsError)
        {
            return validService.FirstError;
        }

        var success = await helper.PerformDataOperation(mappedIncomingService, "add");
        if (success is { IsError: false, Value: true }) return serviceDto;
        
        logger.LogError("Failed to perform data operations on External service {Service}", mappedIncomingService.ServiceName);
        return Errors.DbErrors.AddingToDbFailed;
    }
}

public interface IAddExternalService
{
    public Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto serviceDto);
}