using Bogus;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestManagedEntityService;

public class TestNewsOutletServiceHelper
{
    private static readonly Faker Faker = new();
    public TestNewsOutletServiceHelper()
    {
        Mock<ILogger<ManagedEntityHelper<NewsOutlet>>> mockLogger = new();
        _mockRepository = new Mock<IRepository<NewsOutlet>>();

        _savedNewsOutlets = NewsOutletFixtureBase.Outlets[1];

        BasicSetup();

        _sut = new ManagedEntityHelper<NewsOutlet>(
            mockLogger.Object,
            _mockRepository.Object
        );
    }

    private readonly ManagedEntityHelper<NewsOutlet> _sut;
    private readonly Mock<IRepository<NewsOutlet>> _mockRepository;
    private readonly List<NewsOutlet> _savedNewsOutlets;

    private void BasicSetup()
    {
        _mockRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(_savedNewsOutlets);
    }

    [Fact]
    public async Task MatchNewsOutlets_WhenNoSavedOutlets_ReturnsError()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync((List<NewsOutlet>)null!);

        var incomingOutlets = NewsOutletFixtureBase.Outlets[0];

        // Act
        var result = await _sut.MatchEntities(incomingOutlets);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }

    [Fact]
    public async Task MatchNewsOutlets_WhenAllOutletsMatch_ReturnsAllMatchedNoUnmatched()
    {
        // Arrange

        // Act
        var result = await _sut.MatchEntities(_savedNewsOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        var (matched, unmatched) = result.Value;
        matched.Should().HaveCount(3);
        matched.Select(x => x.Id).Should().BeEquivalentTo(_savedNewsOutlets.Select(x => x.Id));
        unmatched.Should().BeEmpty();
    }

    [Fact]
    public async Task MatchNewsOutlets_WhenNoOutletsMatch_ReturnsAllUnmatchedNoMatched()
    {
        // Arrange
        var incomingOutlets = new List<NewsOutlet>
        {
            new()
            {
                Id = Faker.Random.Int(600),
                Name = Faker.Internet.Url(),
                Website = new Uri(Faker.Internet.Url()),
                MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                NewsPassword =
                    "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                AlwaysJs = Faker.Random.Bool(),
                InUse = Faker.Random.Bool(),
                Themes = []
            },
            new()
            {
                Id = Faker.Random.Int(600),
                Name = Faker.Internet.Url(),
                Website = new Uri(Faker.Internet.Url()),
                MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                NewsPassword =
                    "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                AlwaysJs = Faker.Random.Bool(),
                InUse = Faker.Random.Bool(),
                Themes = []
            },
        };

        // Act
        var result = await _sut.MatchEntities(incomingOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        var (matched, unmatched) = result.Value;
        matched.Should().BeEmpty();
        unmatched.Should().HaveCount(2);
        unmatched.Select(x => x.Id).Should().BeEquivalentTo(incomingOutlets.Select(x => x.Id));
    }

    [Fact]
    public async Task MatchNewsOutlets_WhenSomeOutletsMatch_ReturnsMatchedAndUnmatched()
    {
        // Arrange
        var matchingId = _savedNewsOutlets[0].Id;
        var nonMatchingId = Faker.Random.Int(600);
        var incomingOutlets = new List<NewsOutlet>
        {
            new()
            {
                Id = matchingId,
                Name = Faker.Internet.Url(),
                Website = new Uri(Faker.Internet.Url()),
                MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                NewsPassword =
                    "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                AlwaysJs = Faker.Random.Bool(),
                InUse = Faker.Random.Bool(),
                Themes = []
            },
            new()
            {
                Id = nonMatchingId,
                Name = Faker.Internet.Url(),
                Website = new Uri(Faker.Internet.Url()),
                MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                NewsPassword =
                    "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                AlwaysJs = Faker.Random.Bool(),
                InUse = Faker.Random.Bool(),
                Themes = []
            },
        };

        // Act
        var result = await _sut.MatchEntities(incomingOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        var (matched, unmatched) = result.Value;
        
        matched.Should().ContainSingle();
        matched.Single().Id.Should().Be(matchingId);
        
        unmatched.Should().ContainSingle();
        unmatched.Single().Id.Should().Be(nonMatchingId);
    }

    [Fact]
    public async Task MatchNewsOutlets_WithEmptyIncomingList_ReturnsEmptyMatchedAndUnmatched()
    {
        // Arrange
        var incomingOutlets = new List<NewsOutlet>();

        // Act
        var result = await _sut.MatchEntities(incomingOutlets);

        // Assert
        result.IsError.Should().BeFalse();
        var (matched, unmatched) = result.Value;
        matched.Should().BeEmpty();
        unmatched.Should().BeEmpty();
    }
}