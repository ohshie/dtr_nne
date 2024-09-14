using dtr_nne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestNewsOutletRepository(GenericDatabaseFixture<NewsOutlet> genericDatabaseFixture)
    : IClassFixture<GenericDatabaseFixture<NewsOutlet>>, IDisposable
{
    [Fact]
    public async Task Add_WhenInvoked_AddsNewsOutlet()
    {
        // Assemble
        var newsOutletToBeAdded = NewsOutletFixture.GetTestNewsOutlet().First();

        // Act
        await genericDatabaseFixture.GenericRepository.Add(newsOutletToBeAdded);
        await genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        var addedEntity = genericDatabaseFixture.Context.NewsOutlets.First();
        addedEntity.Should().BeEquivalentTo(newsOutletToBeAdded);
    }

    [Fact]
    public async Task AddRange_WhenInvoked_AddsMultipleOutlets()
    {
        // Assemble
        var newsOutletToBeAdded = NewsOutletFixture.GetTestNewsOutlet();
        
        // Act
        await genericDatabaseFixture.GenericRepository.AddRange(newsOutletToBeAdded);
        await genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Assert
        var addedEntities = await genericDatabaseFixture.Context.NewsOutlets.ToListAsync();
        addedEntities.Should().BeEquivalentTo(newsOutletToBeAdded);
    }
    
    [Fact]
    public async Task GetAll_WhenInvoked_ReturnsExpectedListOfNewsOutlets()
    {
        // Assembl
        var newsOutletToBeAdded = NewsOutletFixture.GetTestNewsOutlet();
        
        await genericDatabaseFixture.GenericRepository.AddRange(newsOutletToBeAdded);
        await genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var newsOutlets = await genericDatabaseFixture.GenericRepository.GetAll() as List<NewsOutlet>;

        // Assert
        newsOutlets.Should().BeOfType<List<NewsOutlet>>();
        newsOutlets.Should().BeEquivalentTo(newsOutletToBeAdded);
    }

    public void Dispose()
    {
        genericDatabaseFixture.Context.Database.EnsureDeleted();
    }
}