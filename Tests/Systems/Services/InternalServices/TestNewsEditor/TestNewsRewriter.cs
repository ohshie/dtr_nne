using dtr_nne.Application.DTO.Article;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.ExternalServices;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.NewsEditor;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Enums;
using dtr_nne.Domain.ExternalServices;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Systems.Services.InternalServices.TestNewsEditor;

public class TestNewsRewriter
{
    public TestNewsRewriter()
    {
        _mockArticleMapper = new Mock<IArticleMapper>();
        _mockServiceProvider = new Mock<IExternalServiceProvider>();
        _mockLlmService = new Mock<ILlmService>();

        var faker = new Bogus.Faker();

        _MockArticleDto = new ArticleDto
        {
            Body = faker.Lorem.Paragraph(),
        };

        _mockArticle = new Article
        {
            Body = _MockArticleDto.Body
        };
        
        BasicSetup();
        
        _sut = new NewsRewriter(new Mock<ILogger<NewsRewriter>>().Object, _mockArticleMapper.Object, _mockServiceProvider.Object);
    }

    private NewsRewriter _sut { get; set; }
    private Mock<IArticleMapper> _mockArticleMapper { get; set; }
    private Mock<IExternalServiceProvider> _mockServiceProvider { get; set; }
    private ArticleDto _MockArticleDto { get; set; }
    private Article _mockArticle { get; set; }
    private Mock<ILlmService> _mockLlmService { get; set; }

    private void BasicSetup()
    {
        _mockServiceProvider.Setup(provider => provider.Provide(ExternalServiceType.Llm, ""))
            .Returns(_mockLlmService.Object);
        
        _mockArticleMapper.Setup(mapper => mapper.DtoToArticle(_MockArticleDto))
            .Returns(_mockArticle);
            
        _mockArticleMapper.Setup(mapper => mapper.ArticleToDto(_mockArticle))
            .Returns(_MockArticleDto);
        
        _mockLlmService.Setup(service => service.ProcessArticleAsync(_mockArticle, ""))
            .ReturnsAsync(_mockArticle);
    }
    
    [Fact]
    public async Task Rewrite_WhenServiceProviderReturnsNull_ReturnsError()
    {
        // Arrange
        _mockServiceProvider
            .Setup(provider => provider.Provide(ExternalServiceType.Llm, ""))
            .Returns((ILlmService)null);

        // Act
        var result = await _sut.Rewrite(_MockArticleDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Service.NoActiveServiceFound);
    }
    
    [Fact]
    public async Task Rewrite_WhenServiceProcessingFails_ReturnsError()
    {
        // Arrange
        _mockLlmService
            .Setup(service => service.ProcessArticleAsync(_mockArticle, ""))
            .ReturnsAsync(Errors.ExternalServiceProvider.Llm.AssistantRunError);

        // Act
        var result = await _sut.Rewrite(_MockArticleDto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ExternalServiceProvider.Llm.AssistantRunError);
    }
    
    [Fact]
    public async Task Rewrite_WhenSuccessful_ReturnsProcessedArticle()
    {
        // Arrange

        // Act
        var result = await _sut.Rewrite(_MockArticleDto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_MockArticleDto);
        _mockServiceProvider.Verify(provider => provider.Provide(ExternalServiceType.Llm, ""), Times.Once);
        _mockArticleMapper.Verify(mapper => mapper.DtoToArticle(_MockArticleDto), Times.Once);
        _mockLlmService.Verify(service => service.ProcessArticleAsync(_mockArticle,""), Times.Once);
        _mockArticleMapper.Verify(mapper => mapper.ArticleToDto(_mockArticle), Times.Once);
    }
    
    [Fact]
    public void RequestService_WhenExceptionOccurs_ReturnsNull()
    {
        // Arrange
        _mockServiceProvider.Setup(provider => provider.Provide(ExternalServiceType.Llm, ""))
            .Throws(new Exception("Service provider error"));

        // Act
        var result = _sut.RequestService();

        // Assert
        result.Should().BeNull();
    }
}