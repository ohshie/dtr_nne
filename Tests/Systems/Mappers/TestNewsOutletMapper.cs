using dtr_nne.Application.DTO;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using Tests.Fixtures;

namespace Tests.Systems.Mappers;

public class TestNewsOutletMapper(
    NewsOutletFixture newsOutletFixture,
    NewsOutletDtoFixture newsOutletDtoFixture)
    : IClassFixture<NewsOutletFixture>, IClassFixture<NewsOutletDtoFixture>
{
    private readonly NewsOutletMapper _sut = new();

    [Fact]
    public void Map_NewsOutletToNewsOutletDto_EnsuresSameIdAndName()
    {
        // assemble
        var newsOutlet = newsOutletFixture.FirstOrDefault()![0] as List<NewsOutlet>;

        // act
        var newsOutletDto = _sut.NewsOutletToNewsOutletDto(newsOutlet[0]);
        
        // assert
        newsOutletDto.Should().BeOfType<NewsOutletDto>();
        newsOutletDto.Id.Should().Match(id => id == newsOutlet[0].Id);
        newsOutletDto.Name.Should().Match(name => name == newsOutlet[0].Name);
    }

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public void Map_NewsOutletListToNewsOutletDtoList_EnsuresSameIds(List<NewsOutlet> newsOutlets)
    {
        // assemble
        var sut = new NewsOutletMapper();

        // act
        var newsOutletDtos = sut.NewsOutletsToNewsOutletsDto(newsOutlets);
        
        // assert
        newsOutletDtos.Should().BeOfType<List<NewsOutletDto>>();
        newsOutletDtos
            .Select(no => no.Id)
            .Should()
            .BeEquivalentTo
                (
                    newsOutlets.Select(n => n.Id)
                );
    }
    
    [Fact]
    public void Map_NewsOutletDtoToNewsOutlet_EnsuresSameIdAndName()
    {
        // assemble
        var newsOutletDto = newsOutletDtoFixture.FirstOrDefault()![0] as List<NewsOutletDto>;

        // act
        var newsOutlet = _sut.NewsOutletDtoToNewsOutlet(newsOutletDto[0]);
        
        // assert
        newsOutlet.Should().BeOfType<NewsOutlet>();
        newsOutlet.Id.Should().Match(id => id == newsOutletDto[0].Id);
        newsOutlet.Name.Should().Match(name => name == newsOutletDto[0].Name);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public void Map_NewsOutletDtosToNewsOutletList_EnsuresSameIdsAndName(List<NewsOutletDto> newsOutletDtos)
    {
        // assemble

        // act
        var newsOutlets = _sut.NewsOutletDtosToNewsOutlets(newsOutletDtos);
        
        // assert
        newsOutlets.Should().BeOfType<List<NewsOutlet>>();
        newsOutlets
            .Select(no => no.Id)
            .Should()
            .BeEquivalentTo
            (
                newsOutletDtos.Select(n => n.Id)
            );
    }

    [Theory]
    [ClassData(typeof(DeleteNewsOutletsDtoFixture))]
    public void Map_DeleteNewsOutletDtosToNewsOutletList_EnsureSameIdsAndName(List<DeleteNewsOutletsDto> incomingNewsOueltDtos)
    {
        // Assemble

        // Act
        var newsOutlets = _sut.DeleteNewsOutletDtosToNewsOutlet(incomingNewsOueltDtos);

        // Assert
        newsOutlets.Should().BeOfType<List<NewsOutlet>>();
        newsOutlets
            .Select(no => no.Id)
            .Should()
            .BeEquivalentTo
            (
                incomingNewsOueltDtos.Select(no => no.Id)
            );
    }
}