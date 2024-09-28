using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.UnitOfWork;

public class TestUnitOfWork(GenericDatabaseFixture<NewsOutlet> genericDatabaseFixture) : IClassFixture<GenericDatabaseFixture<NewsOutlet>>
{
    private readonly Mock<ILogger<UnitOfWork<NneDbContext>>> _logger = new();
    [Fact]
    public async Task Save_Success_SavesTransaction()
    {
        // Arrange
        
        var sut = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, _logger.Object);
        await genericDatabaseFixture.Context.Database.EnsureCreatedAsync();

        // Act
        Func<Task> act = async () => await sut.Save();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Save_WithEmptyDb_IfSavingFails_ShouldThrow()
    {
        // Arrange
        var sut = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, _logger.Object);
        await genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        genericDatabaseFixture.Context.NewsOutlets.Add(new NewsOutlet());
        
        // Act
        Func<Task> act = async () => await sut.Save();
        
        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}