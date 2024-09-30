using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestGenericRepository : IClassFixture<GenericDatabaseFixture<TranslatorApi>>
{
    private readonly GenericDatabaseFixture<TranslatorApi> _genericDatabaseFixture;
    private readonly GenericRepository<TranslatorApi, NneDbContext> _sut;

    private readonly TranslatorApi _mockApi = new TranslatorApi()
    {
        ApiKey = Faker.Name.First()
    };

    public TestGenericRepository(GenericDatabaseFixture<TranslatorApi> genericDatabaseFixture)
    {
        _genericDatabaseFixture = genericDatabaseFixture;
        
        var logger = new Mock<ILogger<GenericRepository<TranslatorApi, NneDbContext>>>();
        
        var unitOfWork = new UnitOfWork<NneDbContext>(genericDatabaseFixture.Context, 
            new Mock<ILogger<UnitOfWork<NneDbContext>>>().Object);
        
        _sut = new GenericRepository<TranslatorApi, NneDbContext>(logger.Object, 
            unitOfWork);
    }

    [Fact]
    public async Task Update_WhenInvoked_UpdatesSavedApiKey()
    {
        // Assemble
        _genericDatabaseFixture.Context.ChangeTracker.Clear();
        await _genericDatabaseFixture.Context.Database.EnsureDeletedAsync();
        await _genericDatabaseFixture.Context.Database.EnsureCreatedAsync();
        _genericDatabaseFixture.Context.Add(_mockApi);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        var keyToUpdate = await _genericDatabaseFixture.Context.TranslatorApis.FirstOrDefaultAsync();
        keyToUpdate!.ApiKey = Faker.Name.Last();

        // Act
        var success = _sut.Update(keyToUpdate);
        await _genericDatabaseFixture.Context.SaveChangesAsync();

        // Assert 
        success.Should().BeTrue();
        var changedEntity = _genericDatabaseFixture.Context.ChangeTracker
            .Entries<TranslatorApi>()
            .Select(e => e.Entity)
            .First();
        changedEntity.Should().BeOfType<TranslatorApi>();
        changedEntity.Should().BeEquivalentTo(keyToUpdate);
    }
}