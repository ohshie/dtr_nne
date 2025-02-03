using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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
        
        // Act
        var result = await sut.Save();
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void Dispose_SetsDisposedFlag()
    {
        // Arrange
        var sut = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, _logger.Object);

        // Act
        sut.Dispose();

        // Assert
        var disposedField = sut.GetType().GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isDisposed = (bool)disposedField?.GetValue(sut)!;
        Assert.True(isDisposed);
    }
    
    [Fact]
    public void Dispose_CallsContextDispose()
    {
        // Arrange
        var contextMock = new Mock<NneDbContext>(new DbContextOptions<NneDbContext>());
        var sut = new UnitOfWork<NneDbContext>(contextMock.Object, _logger.Object);

        // Act
        sut.Dispose();

        // Assert
        contextMock.Verify(c => c.Dispose(), Times.Once);
    }
    
    [Fact]
    public void Dispose_Twice_DoesNotThrow()
    {
        // Arrange
        var sut = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, _logger.Object);

        // Act
        sut.Dispose();
        var exception = Record.Exception(() => sut.Dispose());

        // Assert
        Assert.Null(exception);
    }
}