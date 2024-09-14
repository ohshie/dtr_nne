using System.Runtime.CompilerServices;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

[assembly: InternalsVisibleTo("Tests")]

namespace Tests.Fixtures;

public class GenericDatabaseFixture<TEntity> : IDisposable, IAsyncDisposable where TEntity : class
{
    internal NneDbContext Context { get; private set; }
    internal GenericRepository<TEntity, NneDbContext> GenericRepository { get; private set; }
    
    public GenericDatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<NneDbContext>()
            .UseInMemoryDatabase(databaseName: "testDb")
            .Options;
        Context = new NneDbContext(options);

        var mockLogger = new GenericLoggerFixture<GenericRepository<TEntity, NneDbContext>>();

        var mockUoW = new Mock<IUnitOfWork<NneDbContext>>();
        mockUoW.Setup(uow => uow.Context).Returns(Context);

        GenericRepository = new GenericRepository<TEntity, NneDbContext>
        (
            logger: mockLogger.Logger,
            unitOfWork: mockUoW.Object
        );
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }
}