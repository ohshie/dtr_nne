using dtr_nne.Domain.Entities;
using dtr_nne.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Tests.Systems.DbContext;

public class TestNneDbContext
{
    internal readonly Bogus.Faker Faker = new Bogus.Faker();
    public TestNneDbContext()
    {
        _sut = CreateContext();
    }

    private NneDbContext _sut;
    
    private NneDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new NneDbContext(options);
    }
    
    [Fact]
    public async Task EnsureCreatedAsync_ShouldCreateDatabase()
    {
        // Arrange

        // Act
        await _sut.EnsureCreatedAsync();

        // Assert
        var canConnect = await _sut.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }
    
    [Fact]
    public async Task NewsOutlets_CanAddAndRetrieveEntity()
    {
        // Arrange
        var newsOutlet = new NewsOutlet
        {
            Name = Faker.Name.FindName(),
            Website = new Uri(Faker.Internet.Url()),
            MainPagePassword = Faker.Internet.Protocol(),
            NewsPassword = Faker.Internet.Protocol(),
            Themes = [],
        };

        // Act
        await _sut.NewsOutlets.AddAsync(newsOutlet);
        await _sut.SaveChangesAsync();

        // Assert
        var retrieved = await _sut.NewsOutlets.FirstOrDefaultAsync(n => n.Name == newsOutlet.Name);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(newsOutlet.Name);
    }
    
    [Fact]
    public async Task ExternalServices_CanAddAndRetrieveEntity()
    {
        // Arrange
        var service = new ExternalService
        {
            ServiceName = Faker.Name.FirstName(),
        };

        // Act
        await _sut.ExternalServices.AddAsync(service);
        await _sut.SaveChangesAsync();

        // Assert
        var retrieved = await _sut.ExternalServices.FirstOrDefaultAsync(s => s.ServiceName == service.ServiceName);
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(service, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
                .WhenTypeIs<DateTime>());
    }
    
    [Fact]
    public async Task OpenAiAssistants_CanAddAndRetrieveEntity()
    {
        // Arrange
        var assistant = new OpenAiAssistant
        {
            AssistantId = Faker.Internet.Ipv6(),
        };

        // Act
        await _sut.OpenAiAssistants.AddAsync(assistant);
        await _sut.SaveChangesAsync();

        // Assert
        var retrieved = await _sut.OpenAiAssistants.FirstOrDefaultAsync(a => a.AssistantId == assistant.AssistantId);
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(assistant, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>());
    }
}