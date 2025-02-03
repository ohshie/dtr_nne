using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.EntityManager;

public class ManagedEntityHelper<T>(ILogger<ManagedEntityHelper<T>> logger, 
    IRepository<T> repository) : IManagedEntityHelper<T>
    where T : class, IManagedEntity
{
    public async Task<ErrorOr<(List<T>, List<T>)>> MatchEntities(List<T> incomingEntities)
    {
        if (await repository.GetAll() is not List<T> savedEntities)
        {
            logger.LogError("Currently there are no saved entites of {EntityType} in Db", typeof(T));
            return Errors.ManagedEntities.NotFoundInDb(typeof(T));
        }
        
        var matchedEntities = savedEntities
            .Where(ie => 
                incomingEntities
                    .Select(e => e.Id)
                    .Contains(ie.Id))
            .ToList();
        
        var notMatchedEntities = incomingEntities
            .Where(ie => 
                !savedEntities
                    .Select(e => e.Id)
                    .Contains(ie.Id))
            .ToList();
        
        return (matchedEntities, notMatchedEntities);
    }
}

public interface IManagedEntityHelper<T>
{
    public Task<ErrorOr<(List<T>, List<T>)>> MatchEntities(List<T> incomingEntities);
}