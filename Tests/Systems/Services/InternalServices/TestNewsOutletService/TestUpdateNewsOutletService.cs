using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletDtoFixtures;
using NewsOutletDtoFixture = Tests.Fixtures.NewsOutletDtoFixture;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestUpdateNewsOutletService : BaseTestNewsOutletService
{
    private readonly UpdateNewsOutletService _sut;
    private readonly Mock<INewsOutletServiceHelper> _mockHelpers;

    public TestUpdateNewsOutletService()
    {
        ILogger<UpdateNewsOutletService> logger = new Mock<ILogger<UpdateNewsOutletService>>().Object;

        var newsOutletDtos = NewsOutletDtoFixtureBase.OutletDtos[0];
        
        _mockHelpers = new Mock<INewsOutletServiceHelper>();
        
        Mockrepository
            .Setup(repository => repository.UpdateRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(true);

        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], []));
        
        MockMapper.Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(newsOutletDtos);
        MockMapper
            .Setup(mapper => mapper.DtosToEntities(It.IsAny<List<NewsOutletDto>>()))
            .Returns([new NewsOutlet()]);
        
        _sut = new UpdateNewsOutletService(logger: logger, 
            helper: _mockHelpers.Object,
            repository: Mockrepository.Object, 
            mapper: MockMapper.Object, unitOfWork: MockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task IfPassedEmptyDtoList_ReturnsErrorNoOutletsProvided()
    {
        // Assemble
        
        // Act
        var result = await _sut.UpdateNewsOutlets([]);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NoNewsOutletProvided);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvoked_WithFilledDtos_ShouldCall_MatchNewsOutlets(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert 
        MockMapper.Verify(mapper => mapper.DtosToEntities(newsOutletsDtos), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvoked_WithFiledDtos_ShouldCall_MatchNewsOutlets(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert 
        _mockHelpers.Verify(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvokedProperly_IfDbIsEmpty_ShouldReturn_ErrorNotFoundInDb(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await _sut.UpdateNewsOutlets(newsOutletsDtos);
        
        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NotFoundInDb);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvokedProperly_IfNoneMatched_ShouldReturn_NotMatchedList(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers.Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([], [new NewsOutlet()]));
        MockMapper.Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(newsOutletsDtos);

        // Act
        var result = await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(newsOutletsDtos);
        result.Value.Should().NotBeEmpty();
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task NewsOutletDto_Properly_Matched_ShouldInvoke_RepositoryUpdate(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert
        Mockrepository.Verify(repository => repository.UpdateRange(It.IsAny<List<NewsOutlet>>()), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryError_ReturnsErrorDeletionFailed(
        List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        Mockrepository
            .Setup(repository => repository.UpdateRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(false);
        
        // Act
        var result = await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.UpdateFailed);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryPass_ShouldCallUnitOfWorkSave(List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble

        // Act
        await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert 
        MockUnitOfWork.Verify(uow => uow.Save(), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryPass_ShouldMapNOToDto(
        List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], [new NewsOutlet()]));

        // Act
        await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert 
        MockMapper.Verify(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()),Times.AtLeastOnce);
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task On_PartialSuccess_ShouldReturn_LeftoverDtos(
        List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], [new NewsOutlet()]));

        // Act
        var result = await _sut.UpdateNewsOutlets(newsOutletsDtos);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<NewsOutletDto>>();
        result.Value.Should().HaveCount(1);
    }

    [Theory]
    [ClassData(typeof(NewsOutletDtoFixture))]
    public async Task On_Success_ShouldReturn_EmptyDtos(
        List<NewsOutletDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        var result = await _sut.UpdateNewsOutlets(newsOutletsDtos);
        
        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<NewsOutletDto>>();
        result.Value.Should().HaveCount(0);
    }
}