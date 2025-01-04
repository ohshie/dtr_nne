using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class ExternalServiceProviderRepository(ILogger<ExternalServiceProviderRepository> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) : 
    GenericRepository<ExternalService, NneDbContext>(logger, unitOfWork), 
    IExternalServiceProviderRepository
{
    private readonly IUnitOfWork<NneDbContext> _unitOfWork = unitOfWork;

    public List<ExternalService>? GetByType(ExternalServiceType type)
    {
        try
        {
            var service = _unitOfWork.Context.ExternalServices
                .Where(s => s.Type == type)
                .AsNoTracking()
                .ToList();

            return service;
        }
        catch (Exception e)
        {
            logger.LogError("Something went wrong when trying to fetch external services by type {Type}, " +
                            "{Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                type.GetType(),
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            Console.WriteLine(e);
            return null;
        }
    }
}