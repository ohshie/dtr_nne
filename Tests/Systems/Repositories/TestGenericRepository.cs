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
    private readonly GenericDatabaseFixture<ExternalService> _genericDatabaseFixture;
    private readonly GenericRepository<ExternalService, NneDbContext> _sut;

    private readonly ExternalService _mockService = new()
    {
        ApiKey = Faker.Name.First()
    };

    public TestGenericRepository(GenericDatabaseFixture<ExternalService> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        
        var logger = new Mock<ILogger<GenericRepository<ExternalService, NneDbContext>>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, 
            new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);
        
        _sut = new GenericRepository<ExternalService, NneDbContext>(logger.Object, 
            unitOfWork);
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
        keyToUpdate!.ApiKey = Faker.Name.Last();

        // Act
        var success = _sut.Update(keyToUpdate);
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
}