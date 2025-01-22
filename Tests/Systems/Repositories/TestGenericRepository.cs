using Bogus;
using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestGenericRepository : IClassFixture<GenericDatabaseFixture<ExternalService>>
{
    private readonly static Faker Faker = new Faker();
    private readonly GenericDatabaseFixture<ExternalService> _genericDatabaseFixture;
    private readonly Mock<GenericRepository<ExternalService, NneDbContext>> _sut;
    private readonly Mock<DbSet<ExternalService>> _mockDbSet;

    private readonly ExternalService _mockService = new()
    {
        ApiKey = Faker.Internet.Ipv6()
    };

    public TestGenericRepository(GenericDatabaseFixture<ExternalService> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        _mockDbSet = new();
        
        var logger = new Mock<ILogger<GenericRepository<ExternalService, NneDbContext>>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, 
            new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);
        
        _sut = new Mock<GenericRepository<ExternalService, NneDbContext>>(logger.Object, 
            unitOfWork){CallBase = true};
    }

    [Fact]
    public async Task Update_WhenInvoked_UpdatesSavedApiKey()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(_mockService);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        var keyToUpdate = await _genericDatabaseFixture.Context.ExternalServices.FirstOrDefaultAsync();
        keyToUpdate!.ApiKey = Faker.Internet.Ipv6();

        // Act
        var success = _sut.Object.Update(keyToUpdate);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert 
        success.Should().BeTrue();
        var changedEntity = _genericDatabaseFixture.Context.ChangeTracker
            .Entries<ExternalService>()
            .Select(e => e.Entity)
            .First();
        changedEntity.Should().BeOfType<ExternalService>();
        changedEntity.Should().BeEquivalentTo(keyToUpdate);
    }
    
    [Fact]
    public void Update_WhenError_CatchesAndReturnFalse()
    {
        // Assemble
        _mockDbSet
            .Setup(sut => sut.Update(It.IsAny<ExternalService>())).
            Throws(new Exception());
        _sut.Object.DbSet = _mockDbSet.Object;
        
        // Act
        var result = _sut.Object.Update(_mockService);

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
        await _sut.Object.Add(_mockService);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert 
        var service = await _genericDatabaseFixture.Context.ExternalServices.FirstOrDefaultAsync();
        service.Should().BeEquivalentTo(_mockService);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfEntities()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(_mockService);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Act
        var result = await _sut.Object.GetAll() as List<ExternalService>;

        // Assert 
        result.Should().BeOfType<List<ExternalService>>();
        result!.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Get_ReturnsEntity()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(_mockService);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Act
        var result = await _sut.Object.Get(_mockService.Id);

        // Assert 
        result.Should().NotBeNull();
        result.Should().BeOfType<ExternalService>();
        result!.Id.Should().Be(_mockService.Id);
    }
}