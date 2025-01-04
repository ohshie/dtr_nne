using System.Runtime.CompilerServices;
using dtr_nne.Application.Extensions;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Application.Services.NewsOutletServices;

internal class NewsOutletServiceHelper(ILogger<NewsOutletServiceHelper> logger, INewsOutletRepository repository) : INewsOutletServiceHelper
{
    public async Task<ErrorOr<(List<NewsOutlet>, List<NewsOutlet>)>> MatchNewsOutlets(List<NewsOutlet> incommingNewsOutlets)
    {
        var savedNewsOutlets = await repository.GetAll() as List<NewsOutlet>;
        
        if (savedNewsOutlets is null)
        {
            logger.LogError("Currently there are no saved NewsOutlets in Db");
            return Errors.NewsOutlets.NotFoundInDb;
        }
        
        var matchedNewsOutlets = savedNewsOutlets
            .Where(ino => 
                incommingNewsOutlets
                    .Select(no => no.Id)
                    .Contains(ino.Id))
            .ToList();
        
        var notMatchedNewsOutlets = incommingNewsOutlets
            .Where(ino => 
                !savedNewsOutlets
                    .Select(no => no.Id)
                    .Contains(ino.Id))
            .ToList();
        
        return (matchedNewsOutlets, notMatchedNewsOutlets);
    }
}

public interface INewsOutletServiceHelper
{
    public Task<ErrorOr<(List<NewsOutlet>, List<NewsOutlet>)>> MatchNewsOutlets(List<NewsOutlet> incommingNewsOutlets);
}

