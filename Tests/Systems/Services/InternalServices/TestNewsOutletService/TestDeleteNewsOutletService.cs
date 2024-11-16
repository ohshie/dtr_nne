using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestDeleteNewsOutletService : BaseTestNewsOutletService
{
    private readonly DeleteNewsOutletService _sut;
    private readonly Mock<INewsOutletServiceHelper> _mockHelpers;

    public TestDeleteNewsOutletService()
    {
        ILogger<DeleteNewsOutletService> logger = new Mock<ILogger<DeleteNewsOutletService>>().Object;
        
        _mockHelpers = new Mock<INewsOutletServiceHelper>();

        _mockHelpers.Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], []));
        Mockrepository
            .Setup(repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(true);
        MockMapper.Setup(mapper => mapper.EntitiesToBaseDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns([new BaseNewsOutletsDto()]);
        
        _sut = new DeleteNewsOutletService(logger: logger, 
            helper: _mockHelpers.Object,
            repository: Mockrepository.Object, 
            mapper: MockMapper.Object, unitOfWork: MockUnitOfWork.Object);
    }

    [Fact]
    public async Task IfPassedEmptyDtoList_ReturnsErrorNoOutletsProvided()
    {
        // Assemble
        
        // Act
        var result = await _sut.DeleteNewsOutlets([]);

        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NoNewsOutletProvided);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvoked_WithFilledDtos_ShouldCall_MatchNewsOutlets(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble

        // Act
        await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert 
        MockMapper.Verify(mapper => mapper.BaseDtosToEntities(newsOutletsDtos), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvoked_WithFiledDtos_ShouldCall_MatchNewsOutlets(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert 
        _mockHelpers.Verify(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_IfDbIsEmpty_ShouldReturn_ErrorNotFoundInDb(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await _sut.DeleteNewsOutlets(newsOutletsDtos);
        
        // Assert 
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NotFoundInDb);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_IfNoneMatched_ShouldReturn_NotMatchedList(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers.Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([], [new NewsOutlet()]));
        MockMapper.Setup(mapper => mapper.EntitiesToBaseDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(newsOutletsDtos);

        // Act
        var result = await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(newsOutletsDtos);
        result.Value.Should().NotBeEmpty();
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_Matched_ShouldInvoke_RepositoryUpdate(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert
        Mockrepository.Verify(repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>()), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryError_ReturnsErrorDeletionFailed(
        List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        Mockrepository
            .Setup(repository => repository.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(false);
        
        // Act
        var result = await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.DeletionFailed);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryPass_ShouldCallUnitOfWorkSave(List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble

        // Act
        await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert 
        MockUnitOfWork.Verify(uow => uow.Save(), Times.AtLeastOnce);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task WhenInvokedProperly_Matched_RepositoryPass_ShouldMapNOToDto(
        List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], [new NewsOutlet()]));

        // Act
        await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert 
        MockMapper.Verify(mapper => mapper.EntitiesToBaseDtos(It.IsAny<List<NewsOutlet>>()),Times.AtLeastOnce);
    }
    
    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task On_PartialSuccess_ShouldReturn_LeftoverDtos(
        List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        _mockHelpers
            .Setup(helper => helper.MatchNewsOutlets(It.IsAny<List<NewsOutlet>>()).Result)
            .Returns(([new NewsOutlet()], [new NewsOutlet()]));

        // Act
        var result = await _sut.DeleteNewsOutlets(newsOutletsDtos);

        // Assert 
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<BaseNewsOutletsDto>>();
        result.Value.Should().HaveCount(1);
    }

    [Theory]
    [ClassData(typeof(BaseNewsOutletsDtoFixture))]
    public async Task On_Success_ShouldReturn_EmptyDtos(
        List<BaseNewsOutletsDto> newsOutletsDtos)
    {
        // Assemble
        
        // Act
        var result = await _sut.DeleteNewsOutlets(newsOutletsDtos);
        
        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<List<BaseNewsOutletsDto>>();
        result.Value.Should().HaveCount(0);
    }
}