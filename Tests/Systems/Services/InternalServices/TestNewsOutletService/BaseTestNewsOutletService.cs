using dtr_nne.Application.Mapper;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public abstract class BaseTestNewsOutletService
{
    internal readonly Mock<INewsOutletRepository> MockNewsOutletRepository = new();
    internal readonly INewsOutletMapper Mapper = new NewsOutletMapper();
    internal readonly Mock<IUnitOfWork<INneDbContext>> MockUnitOfWork = new();
}