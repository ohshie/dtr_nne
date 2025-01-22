using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.Repositories;

internal class NewsOutletRepository(ILogger<GenericRepository<NewsOutlet, NneDbContext>> logger, 
    IUnitOfWork<NneDbContext> unitOfWork) 
    : GenericRepository<NewsOutlet, NneDbContext>(logger, unitOfWork), INewsOutletRepository
{
    private readonly ILogger<GenericRepository<NewsOutlet, NneDbContext>> _logger = logger;
    private readonly IUnitOfWork<NneDbContext> _unitOfWork = unitOfWork;

    public bool UpdateRange(IEnumerable<NewsOutlet> incomingNewsOutlets)
    {
        try
        {
            _unitOfWork.Context.UpdateRange(incomingNewsOutlets);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to pass Update Range on {TypeOfEntity}, Exception: {Exception}, \n" +
                            "{Message}\n" +
                            "{StackTrace}", 
                incomingNewsOutlets.GetType(),
                e.InnerException?.Message ?? "No Inner Exception",
                e.Message,
                e.StackTrace);
            throw;
        }
    }

    public bool RemoveRange(IEnumerable<NewsOutlet> incomingNewsOutlets)
    {
        try
        {
            _unitOfWork.Context.RemoveRange(incomingNewsOutlets);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to pass Remove Range on {TypeOfEntity}, " +
                            "Exception: {Exception}, \n" +
                            "{Message}\n" +
                            "{StackTrace}", 
                incomingNewsOutlets.GetType(),
                e.InnerException?.Message ?? "No Inner Exception",
                e.Message,
                e.StackTrace);
            throw;
        }
    }
}