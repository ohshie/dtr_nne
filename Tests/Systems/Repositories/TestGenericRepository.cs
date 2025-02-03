using Bogus;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Repositories;

public class TestGenericRepository : IClassFixture<GenericDatabaseFixture<NewsOutlet>>
{
    private static readonly Faker Faker = new Faker();
    private readonly GenericDatabaseFixture<NewsOutlet> _genericDatabaseFixture;
    private readonly Mock<GenericRepository<NewsOutlet, NneDbContext>> _sut;
    private readonly Mock<DbSet<NewsOutlet>> _mockDbSet;

    public TestGenericRepository(GenericDatabaseFixture<NewsOutlet> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        _mockDbSet = new();
        
        var logger = new Mock<ILogger<GenericRepository<NewsOutlet, NneDbContext>>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, 
            new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);
        
        _sut = new Mock<GenericRepository<NewsOutlet, NneDbContext>>(logger.Object, 
            unitOfWork){CallBase = true};
    }

    [Fact]
    public async Task Update_WhenInvoked_UpdatesSavedApiKey()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(NewsOutletFixtureBase.Outlets[0][0]);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        var nameToUpdate = await _genericDatabaseFixture.Context.NewsOutlets.FirstOrDefaultAsync();
        nameToUpdate!.Name = Faker.Lorem.Word();

        // Act
        var success = _sut.Object.Update(nameToUpdate);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert 
        success.Should().BeTrue();
        var changedEntity = _genericDatabaseFixture.Context.ChangeTracker
            .Entries<NewsOutlet>()
            .Select(e => e.Entity)
            .First();
        changedEntity.Should().BeOfType<NewsOutlet>();
        changedEntity.Should().BeEquivalentTo(nameToUpdate);
    }
    
    [Fact]
    public void Update_WhenError_CatchesAndReturnFalse()
    {
        // Assemble
        _mockDbSet
            .Setup(sut => sut.Update(It.IsAny<NewsOutlet>())).
            Throws(new Exception());
        _sut.Object.DbSet = _mockDbSet.Object;
        
        // Act
        var result = _sut.Object.Update(NewsOutletFixtureBase.Outlets[0][0]);

        // Assert 
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task Add_AddsEntity()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();

        // Act
        await _sut.Object.Add(NewsOutletFixtureBase.Outlets[0][0]);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert 
        var service = await _genericDatabaseFixture.Context.NewsOutlets.FirstOrDefaultAsync();
        service.Should().BeEquivalentTo(NewsOutletFixtureBase.Outlets[0][0]);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfEntities()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(NewsOutletFixtureBase.Outlets[0][0]);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Act
        var result = await _sut.Object.GetAll() as List<NewsOutlet>;

        // Assert 
        result.Should().BeOfType<List<NewsOutlet>>();
        result!.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Get_ReturnsEntity()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(NewsOutletFixtureBase.Outlets[0][0]);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Act
        var result = await _sut.Object.Get(NewsOutletFixtureBase.Outlets[0][0].Id);

        // Assert 
        result.Should().NotBeNull();
        result.Should().BeOfType<NewsOutlet>();
        result!.Id.Should().Be(NewsOutletFixtureBase.Outlets[0][0].Id);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task AddRange_WhenInvoked_ReturnsTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        await CleanUp();
        
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
    public async Task AddRange_WithNullEntities_ReturnsFalse()
    {
        // Arrange
        IEnumerable<NewsOutlet> entities = null!;

        // Act
        var result = await _genericDatabaseFixture.Repository.AddRange(entities);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateRange_WithNullEntities_ShouldThrowException()
    {
        // Assemble
        IEnumerable<NewsOutlet> entities = null!;
        
        // Act
        var act = () => _sut.Object.UpdateRange(entities);
        
        // Assert 
        act.Should().Throw<NullReferenceException>();
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task UpdateRange_WhenInvokedWithProperList_ReturnsTrue(List<NewsOutlet> newsOutlets)
    {
        // Assemble
        await CleanUp();
        await _genericDatabaseFixture.Context.AddRangeAsync(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var success = _sut.Object.UpdateRange(newsOutlets);
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
        await CleanUp();
        await _genericDatabaseFixture.Context.AddRangeAsync(newsOutlets);
        await _genericDatabaseFixture.Context.SaveChangesAsync();
        
        // Act
        var success = _sut.Object.RemoveRange(newsOutlets);

        // Assert 
        success.Should().BeTrue();
    }
    
    [Fact]
    public void RemoveRange_WithNullEntities_ShouldThrowException()
    {
        // Assemble
        IEnumerable<NewsOutlet> entities = null!;
        
        // Act
        var act = () => _sut.Object.RemoveRange(entities);
        
        // Assert 
        act.Should().Throw<NullReferenceException>();
    }

    private async Task CleanUp()
    {
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
    }
}