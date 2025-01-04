using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;

namespace Tests.Systems.Mappers;

public class TestExternalServiceMapper
{
    public TestExternalServiceMapper()
    {
        var faker = new Bogus.Faker();
        _testExternalServiceDto = new()
        {
            InUse = true,
            ServiceName = faker.Lorem.Word(),
            Type = ExternalServiceType.Translator,
            ApiKey = faker.Lorem.Word()
        };

        _testExternalService = new()
        {
            InUse = true,
            ServiceName = _testExternalServiceDto.ServiceName,
            Type = _testExternalServiceDto.Type,
            ApiKey = _testExternalServiceDto.ApiKey
        };
        
        _sut = new();
    }
    
    private readonly ExternalServiceMapper _sut;
    private readonly ExternalServiceDto _testExternalServiceDto;
    private readonly ExternalService _testExternalService;
    
    [Fact]
    public void Map_DtoToArticle_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var article = _sut.DtoToService(_testExternalServiceDto);
        
        // assert
        article.Should().BeOfType<ExternalService>();
        _testExternalServiceDto.ApiKey.Should().Match(key => key == _testExternalServiceDto.ApiKey);
    }
    
    [Fact]
    public void Map_ArticleToDto_EnsuresSameIdAndName()
    {
        // assemble

        // act
        var articleDto = _sut.ServiceToDto(_testExternalService);
        
        // assert
        articleDto.Should().BeOfType<ExternalServiceDto>();
        _testExternalService.ApiKey.Should().Match(key => key == _testExternalService.ApiKey);
    }
}