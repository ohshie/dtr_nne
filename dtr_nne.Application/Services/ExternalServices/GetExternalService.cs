using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.ExternalServices;

internal class GetExternalService(ILogger<GetExternalService> logger, 
    IExternalServiceMapper mapper,
    IExternalServiceProviderRepository repository) : IGetExternalService
{
    public async Task<ErrorOr<List<ExternalServiceDto>>> GetAllByType(ExternalServiceType type)
    {
        logger.LogInformation("Starting Add method for service: {ServiceType}", type);

        if (await repository.GetByType(type) is not { } registeredServices)
        {
            logger.LogWarning("No registered services of type: {ServiceType} were found in Db", type);
            return Errors.ExternalServiceProvider.Service.NoSavedServiceFound;
        }
        
        logger.LogInformation("Found {AmountServiceByType} registered services of {ServiceType} type", registeredServices.Count, type);

        var mappedRegisteredServices = mapper.ServiceToDto(registeredServices);

        return mappedRegisteredServices;
    }
}

public interface IGetExternalService
{
    public Task<ErrorOr<List<ExternalServiceDto>>> GetAllByType(ExternalServiceType type);
}