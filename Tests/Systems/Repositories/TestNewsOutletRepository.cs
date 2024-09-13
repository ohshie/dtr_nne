using Microsoft.EntityFrameworkCore;
using Tests.Fixtures;

namespace Tests.Systems.Repositories;

public class TestNewsOutletRepository
{
    [Fact]
    public async Task Add_WhenInvoked_AddsNewsOutlet()
    {
        // Assemble
        var newsOutletFixture = new NewsOutletFixture();
        var (newsOutletRepository, context) = newsOutletFixture.ProvideEmptyRepository();
        var newsOutletToBeAdded = NewsOutletFixture.GetTestNewsOutlet().First();

        // Act
        await newsOutletRepository.Add(newsOutletToBeAdded);
        await context.SaveChangesAsync();

        // Assert
        var addedEntity = context.NewsOutlets.First();
        addedEntity.Should().BeSameAs(newsOutletToBeAdded);
    }

    [Fact]
    public async Task AddRange_WhenInvoked_AddsMultipleOutlets()
    {
        // Assemble
        var newsOutletFixture = new NewsOutletFixture();
        var (newsOutletRepository, context) = newsOutletFixture.ProvideEmptyRepository();
        var newsOutletToBeAdded = NewsOutletFixture.GetTestNewsOutlet();
        
        // Act
        await newsOutletRepository.AddRange(newsOutletToBeAdded);
        await context.SaveChangesAsync();
        
        // Assert
        var addedEntities = await context.NewsOutlets.ToListAsync();
        addedEntities.Should().BeSameAs(newsOutletToBeAdded);
    }
}