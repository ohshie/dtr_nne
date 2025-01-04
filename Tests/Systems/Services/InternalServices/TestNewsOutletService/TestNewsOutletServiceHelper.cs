using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestNewsOutletServiceHelper
{
    public TestNewsOutletServiceHelper()
    {
        var faker = new Bogus.Faker();
        
        _mockRepository = new();
        _testSavedNewsOutlets = new List<NewsOutlet>([
            new NewsOutlet()
            {
                Id = 1,
                Name = faker.Lorem.Word()
            },
            new NewsOutlet()
            {
                Id = 2,
                Name = faker.Lorem.Word()
            }
        ]);

        _testIncomingNewsOutlets = new List<NewsOutlet>([
            new NewsOutlet()
            {
                Id = 1,
                Name = _testSavedNewsOutlets[0].Name
            }
        ]);

        _mockRepository
            .Setup(repository => repository.GetAll().Result)
            .Returns(_testSavedNewsOutlets);
        
        _sut = new(new Mock<ILogger<NewsOutletServiceHelper>>().Object, _mockRepository.Object);
    }
    private readonly NewsOutletServiceHelper _sut;
    private readonly Mock<INewsOutletRepository> _mockRepository;

    private List<NewsOutlet> _testSavedNewsOutlets;
    private List<NewsOutlet> _testIncomingNewsOutlets;

    [Fact]
    public async Task MatchNewsOutlets_IfCorrect_ReturnsMatched()
    {
        // Assemble

        // Act
        var results = await _sut.MatchNewsOutlets(_testIncomingNewsOutlets);

        // Assert 
        results.IsError.Should().BeFalse();
        var (matchedNewsOutlets, notMatchedNewsOutlets) = results.Value;
        matchedNewsOutlets.Count.Should().Be(1);
        notMatchedNewsOutlets.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task MatchNewsOutlets_IfNoneMatched_ReturnsNotMatched()
    {
        // Assemble
        _testIncomingNewsOutlets[0].Id = 3;
        
        // Act
        var results = await _sut.MatchNewsOutlets(_testIncomingNewsOutlets);

        // Assert 
        results.IsError.Should().BeFalse();
        var (matchedNewsOutlets, notMatchedNewsOutlets) = results.Value;
        matchedNewsOutlets.Count.Should().Be(0);
        notMatchedNewsOutlets.Count.Should().Be(1);
    }

    [Fact]
    public async Task MatchNewsOutlets_IfNoneInDb_ShouldReturnError()
    {
        // Assemble
        _mockRepository
            .Setup(repository => repository.GetAll().Result)
            .Returns((IEnumerable<NewsOutlet>?)null);

        // Act
        var result = await _sut.MatchNewsOutlets(_testIncomingNewsOutlets);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.NewsOutlets.NotFoundInDb);
    }
}