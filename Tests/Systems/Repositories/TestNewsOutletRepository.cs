using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Repositories;

public class TestNewsOutletRepository : IClassFixture<GenericDatabaseFixture<NewsOutlet>>
{
    private readonly GenericDatabaseFixture<NewsOutlet> _genericDatabaseFixture;
    private readonly NewsOutletRepository _newsOutletRepository;

    public TestNewsOutletRepository(GenericDatabaseFixture<NewsOutlet> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        
        var logger = new Mock<ILogger<NewsOutletRepository>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);

        _newsOutletRepository = new NewsOutletRepository(logger.Object, 
            unitOfWork);
    }

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task Add_WhenInvoked_AddsNewsOutlet(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        var newsOutletToBeAdded = newsOutlets.First();
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        
        // Act
        await _genericDatabaseFixture.Repository.Add(newsOutletToBeAdded);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        var addedEntity = _genericDatabaseFixture.Context.NewsOutlets.First();
        addedEntity.Should().BeEquivalentTo(newsOutletToBeAdded);
    }

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task AddRange_WhenInvoked_AddsMultipleOutlets(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        
        // Act
        await _genericDatabaseFixture.Repository.AddRange(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Assert
        var addedEntities = await _genericDatabaseFixture.Context.NewsOutlets.ToListAsync();
        addedEntities.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task GetAll_WhenInvoked_ReturnsExpectedListOfNewsOutlets(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        
        await _genericDatabaseFixture.Repository.AddRange(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var savedNewsOutlets = await _genericDatabaseFixture.Repository.GetAll() as List<NewsOutlet>;

        // Assert
        savedNewsOutlets.Should().BeOfType<List<NewsOutlet>>();
        savedNewsOutlets.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task AddRange_WhenInvoked_ReturnsTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        
        // Act
        var success = await _genericDatabaseFixture.Repository.AddRange(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        success.Should().BeTrue();
        var trackedEntries = _genericDatabaseFixture.Context.ChangeTracker
            .Entries<NewsOutlet>()
            .Select(e => e.Entity)
            .ToList();
        trackedEntries.Should().BeOfType<List<NewsOutlet>>();
        trackedEntries.Should().HaveCount(newsOutlets.Count);
        trackedEntries.Should().BeEquivalentTo(newsOutlets);
    }
    
    [Fact]
    public async Task AddRange_WithNullEntities_ShouldThrowException()
    {
        // Arrange
        IEnumerable<NewsOutlet> entities = null!;

        // Act
        Func<Task> act = async () => await _genericDatabaseFixture.Repository.AddRange(entities);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public void UpdateRange_WithNullEntities_ShouldThrowException()
    {
        // Assemble
        IEnumerable<NewsOutlet> entities = null!;
        
        // Act
        var act = () => _newsOutletRepository.UpdateRange(entities);
        
        // Assert 
        act.Should().Throw<NullReferenceException>();
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task UpdateRange_WhenInvokedWithProperList_ReturnsTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        await _genericDatabaseFixture.Context.AddRangeAsync(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var success = _newsOutletRepository.UpdateRange(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert
        success.Should().BeTrue();
        var trackedEntries = _genericDatabaseFixture.Context.ChangeTracker
            .Entries<NewsOutlet>()
            .Select(e => e.Entity)
            .ToList();
        trackedEntries.Should().BeOfType<List<NewsOutlet>>();
        trackedEntries.Should().HaveCount(newsOutlets.Count);
        trackedEntries.Should().BeEquivalentTo(newsOutlets);
    }

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task DeleteRange_WhenInvokedProperly_ShouldReturnTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        await _genericDatabaseFixture.Context.AddRangeAsync(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var success = _newsOutletRepository.RemoveRange(newsOutlets);

        // Assert 
        success.Should().BeTrue();
    }
    
    [Fact]
    public void RemoveRange_WithNullEntities_ShouldThrowException()
    {
        // Assemble
        IEnumerable<NewsOutlet> entities = null!;
        
        // Act
        var act = () => _newsOutletRepository.RemoveRange(entities);
        
        // Assert 
        act.Should().Throw<NullReferenceException>();
    }
}