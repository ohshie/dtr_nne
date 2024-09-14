using dtr_nne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestNewsOutletRepository(GenericDatabaseFixture<NewsOutlet> genericDatabaseFixture)
    : IClassFixture<GenericDatabaseFixture<NewsOutlet>>, IDisposable
{
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task Add_WhenInvoked_AddsNewsOutlet(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        var newsOutletToBeAdded = newsOutlets.First();
        
        // Act
        await genericDatabaseFixture.GenericRepository.Add(newsOutletToBeAdded);
        await genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        var addedEntity = genericDatabaseFixture.Context.NewsOutlets.First();
        addedEntity.Should().BeEquivalentTo(newsOutletToBeAdded);
    }

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task AddRange_WhenInvoked_AddsMultipleOutlets(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        
        // Act
        await genericDatabaseFixture.GenericRepository.AddRange(newsOutlets);
        await genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Assert
        var addedEntities = await genericDatabaseFixture.Context.NewsOutlets.ToListAsync();
        addedEntities.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task GetAll_WhenInvoked_ReturnsExpectedListOfNewsOutlets(List<NewsOutlet> newsOutlets)
    {
        
        // Assemble
        await genericDatabaseFixture.GenericRepository.AddRange(newsOutlets);
        await genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var savedNewsOutlets = await genericDatabaseFixture.GenericRepository.GetAll() as List<NewsOutlet>;

        // Assert
        savedNewsOutlets.Should().BeOfType<List<NewsOutlet>>();
        savedNewsOutlets.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task AddRange_WhenInvoked_ReturnsTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        
        // Act
        var success = await genericDatabaseFixture.GenericRepository.AddRange(newsOutlets);
        await genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        success.Should().BeTrue();
        var trackedEntries = genericDatabaseFixture.Context.ChangeTracker
            .Entries<NewsOutlet>()
            .Select(e => e.Entity)
            .ToList();
        trackedEntries.Should().BeOfType<List<NewsOutlet>>();
        trackedEntries.Should().HaveCount(newsOutlets.Count);
        trackedEntries.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Fact]
    public async Task AddRange_WithNullEntities_ShouldLogErrorAndThrowException()
    {
        // Arrange
        IEnumerable<NewsOutlet> entities = null!;

        // Act
        Func<Task> act = async () => await genericDatabaseFixture.GenericRepository.AddRange(entities);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    public void Dispose()
    {
        genericDatabaseFixture.Context.ChangeTracker.Clear();
        genericDatabaseFixture.Context.Database.EnsureDeleted();
    }
}