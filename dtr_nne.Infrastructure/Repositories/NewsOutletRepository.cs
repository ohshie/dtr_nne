using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class NewsOutletRepository(ILogger<GenericRepository<NewsOutlet, NneDbContext>> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) 
    : GenericRepository<NewsOutlet, NneDbContext>(logger, unitOfWork), INewsOutletRepository
{
    public bool UpdateRange(IEnumerable<NewsOutlet> incomingNewsOutlet)
    {
        try
        {
            unitOfWork.Context.UpdateRange(incomingNewsOutlet);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to pass Update Range on {TypeOfEntity}, Exception: {Exception}, \n" +
                            "{Message}\n" +
                            "{StackTrace}", 
                incomingNewsOutlet.GetType(),
                e.InnerException,
                e.Message,
                e.StackTrace);
            throw;
        }
    }
}