using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestManagedEntityService;

public abstract class BaseTestManagedEntityService
{
    internal readonly Mock<IRepository<NewsOutlet>> Mockrepository = new();
    internal readonly Mock<IManagedEntityMapper> MockMapper = new();
    internal readonly Mock<IUnitOfWork<INneDbContext>> MockUnitOfWork = new();
}