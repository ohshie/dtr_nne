using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using Tests.Fixtures;
using Tests.Fixtures.NewsOutletDtoFixtures;
using Tests.Fixtures.NewsOutletFixtures;
using NewsOutletDtoFixture = Tests.Fixtures.NewsOutletDtoFixture;

namespace Tests.Systems.Mappers;

public class TestNewsOutletMapper
    : IClassFixture<NewsOutletFixture>, IClassFixture<NewsOutletDtoFixture>
{
    private readonly NewsOutletMapper _sut = new();

    [Fact]
    public void Map_NewsOutletToNewsOutletDto_EnsuresSameIdAndName()
    {
        // assemble
        var newsOutlet = NewsOutletFixtureBase.Outlets[0];

        // act
        var newsOutletDto = _sut.EntityToDto(newsOutlet[0]);
        
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
    
    [Fact]
    public void Map_NewsOutletDtoToNewsOutlet_EnsuresSameIdAndName()
    {
        // assemble
        var newsOutletDto = NewsOutletDtoFixtureBase.OutletDtos[0];

        // act
        var newsOutlet = _sut.DtoToEntity(newsOutletDto[0]);
        
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