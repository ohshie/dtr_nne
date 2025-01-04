using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using Tests.Fixtures;
using Tests.Fixtures.NewsOutletFixtures;
using NewsOutletDtoFixture = Tests.Fixtures.NewsOutletDtoFixture;

namespace Tests.Systems.Mappers;

public class TestNewsOutletMapper
    : IClassFixture<NewsOutletFixture>, IClassFixture<NewsOutletDtoFixture>
{
    private readonly NewsOutletMapper _sut = new();

    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public void Map_NewsOutletListToNewsOutletDtoList_EnsuresSameIds(List<NewsOutlet> newsOutlets)
    {
        // assemble
        var sut = new NewsOutletMapper();

        // act
        var newsOutletDtos = sut.EntitiesToDtos(newsOutlets);
        
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

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public void Map_NewsOutletDtosToNewsOutletList_EnsuresSameIdsAndName(List<NewsOutletDto> newsOutletDtos)
    {
        // assemble

        // act
        var newsOutlets = _sut.DtosToEntities(newsOutletDtos);
        
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
    [ClassData(typeof(NewsOutletFixture))]
    public void Map_NewsOutletsToNewsOutletBaseDtos_EnsuresSameIdsAndName(List<NewsOutlet> newsOutlets)
    {
        // assemble

        // act
        var baseNewsOutletDtos = _sut.EntitiesToBaseDtos(newsOutlets);
        
        // assert
        baseNewsOutletDtos.Should().BeOfType<List<BaseNewsOutletsDto>>();
        baseNewsOutletDtos
            .Select(no => no.Id)
            .Should()
            .BeEquivalentTo
            (
                newsOutlets.Select(n => n.Id)
            );
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public void Map_DeleteNewsOutletDtosToNewsOutletList_EnsureSameIdsAndName(List<BaseNewsOutletsDto> incomingNewsOueltDtos)
    {
        // Assemble

        // Act
        var newsOutlets = _sut.BaseDtosToEntities(incomingNewsOueltDtos);

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