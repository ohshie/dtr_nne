using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsOutletServices;

public class DeleteNewsOutletService(ILogger<DeleteNewsOutletService> logger, 
    INewsOutletRepository repository, 
    INewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : IDeleteNewsOutletService
{
    public async Task<ErrorOr<List<NewsOutletDto>>> DeleteNewsOutlets(List<BaseNewsOutletsDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to delete. Returning");
            return Errors.NewsOutlets.NoNewsOutletProvided;
        }

        var mappedIncomingNewsOutlets = mapper.DeleteNewsOutletDtosToNewsOutlet(incomingNewsOutletDtos);

        var savedNewsOutlets = await repository.GetAll() as List<NewsOutlet>;
        
        if (savedNewsOutlets is null)
        {
            logger.LogError("Currently there are no saved NewsOutlets in Db");
            return Errors.NewsOutlets.NotFoundInDb;
        }
        
        var matchedNewsOutlets = savedNewsOutlets
            .Where(mino => 
                mappedIncomingNewsOutlets
                    .Select(no => no.Id)
                    .Contains(mino.Id))
            .ToList();
        
        var notMatchedNewsOutlets = mappedIncomingNewsOutlets
            .Where(mino => 
                !savedNewsOutlets
                    .Select(no => no.Id)
                    .Contains(mino.Id))
            .ToList();

        var success = repository.RemoveRange(matchedNewsOutlets);
        if (!success)
        {
            return Errors.NewsOutlets.DeletionFailed;
        }

        await unitOfWork.Save();
        
        var notDeletedNewsOutletDtos = mapper.NewsOutletsToNewsOutletsDto(notMatchedNewsOutlets); 
        
        return notDeletedNewsOutletDtos;
    }
}

public interface IDeleteNewsOutletService
{
    Task<ErrorOr<List<NewsOutletDto>>> DeleteNewsOutlets(List<BaseNewsOutletsDto> incomingNewsOutletDtos);
}