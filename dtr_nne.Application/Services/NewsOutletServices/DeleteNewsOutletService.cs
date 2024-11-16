using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace dtr_nne.Application.Services.NewsOutletServices;

internal class DeleteNewsOutletService(ILogger<DeleteNewsOutletService> logger, 
    INewsOutletServiceHelper helper,
    INewsOutletRepository repository,
    INewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : IDeleteNewsOutletService
{
    public async Task<ErrorOr<List<BaseNewsOutletsDto>>> DeleteNewsOutlets(List<BaseNewsOutletsDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to delete. Returning");
            return Errors.NewsOutlets.NoNewsOutletProvided;
        }

        var mappedIncomingNewsOutlets = mapper.BaseDtosToEntities(incomingNewsOutletDtos);
        
        var processedOutlets = await helper.MatchNewsOutlets(mappedIncomingNewsOutlets);
        if (processedOutlets.IsError)
        {
            return processedOutlets.FirstError;
        }

        var (matchedNewsOutlets, notMatchedNewsOutlets) = processedOutlets.Value;
        if (matchedNewsOutlets.Count < 1)
        {
            logger.LogError("No provided entities found in Db, returning list back to the customer");
            var notMatchedNewsOutletDtos = mapper.EntitiesToBaseDtos(notMatchedNewsOutlets);
            return notMatchedNewsOutletDtos;
        }
        
        var success = repository.RemoveRange(matchedNewsOutlets);
        if (!success)
        {
            logger.LogError("Removing range of entities from Db resulted in Error");
            return Errors.NewsOutlets.DeletionFailed;
        }

        await unitOfWork.Save();
        
        if (notMatchedNewsOutlets.Count <  1)
        {
            logger.LogInformation("Successfully removed all provided entities from Db");
            return new List<BaseNewsOutletsDto>{Capacity = 0};
        }

        logger.LogWarning(
            "Partially removed provided entities from Db, returning {NotMatchedOutletsCount} Dtos for customer to check",
            notMatchedNewsOutlets.Count);
        var notDeletedNewsOutletDtos = mapper.EntitiesToBaseDtos(notMatchedNewsOutlets); 
        return notDeletedNewsOutletDtos;
    }
}

public interface IDeleteNewsOutletService
{
    Task<ErrorOr<List<BaseNewsOutletsDto>>> DeleteNewsOutlets(List<BaseNewsOutletsDto> incomingNewsOutletDtos);
}