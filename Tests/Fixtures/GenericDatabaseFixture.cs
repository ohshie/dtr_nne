using System.Data.Common;
using System.Runtime.CompilerServices;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

[assembly: InternalsVisibleTo("Tests")]

namespace Tests.Fixtures;

public class GenericDatabaseFixture<TEntity> : IDisposable, IAsyncDisposable where TEntity : class
{
    private readonly DbConnection _connection;
    internal NneDbContext Context { get; }
    internal GenericRepository<TEntity, NneDbContext> Repository { get; private set; }
    
    public GenericDatabaseFixture()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        
        var contextOptions = new DbContextOptionsBuilder<NneDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new NneDbContext(contextOptions);
        
        var mockLogger = new GenericLoggerFixture<GenericRepository<TEntity, NneDbContext>>();

        var mockUoW = new Mock<IUnitOfWork<NneDbContext>>();
        mockUoW.Setup(uow => uow.Context).Returns(Context);

        Repository = new GenericRepository<TEntity, NneDbContext>
        (
            logger: mockLogger.Logger,
            unitOfWork: mockUoW.Object
        );
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        Context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await Context.DisposeAsync();
    }
}