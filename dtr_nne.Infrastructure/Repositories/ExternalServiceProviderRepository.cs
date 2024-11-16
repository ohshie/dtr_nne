using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class ExternalServiceProviderRepository(ILogger<ExternalServiceProviderRepository> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) : 
    GenericRepository<ExternalService, NneDbContext>(logger, unitOfWork), 
    IExternalServiceProviderRepository
{
    public Task<IEnumerable<ExternalService>?> GetByType(ExternalServiceType type)
    {
        throw new NotImplementedException();
    }
}