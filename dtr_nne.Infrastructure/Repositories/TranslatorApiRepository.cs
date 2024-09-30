using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class TranslatorApiRepository(ILogger<TranslatorApiRepository> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) : GenericRepository<TranslatorApi, NneDbContext>(logger, unitOfWork), ITranslatorApiRepository
{
}