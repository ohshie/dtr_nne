using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletDtoFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestNewsOutletService;

public class TestDeleteNewsOutletService
{
    public TestDeleteNewsOutletService()
    {
        Mock<ILogger<DeleteNewsOutletService>> mockLogger = new();
        _mockHelper = new Mock<INewsOutletServiceHelper>();
        _mockRepository = new Mock<INewsOutletRepository>();
        _mockMapper = new Mock<INewsOutletMapper>();
        _mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();

        _mockNewsOutletDtos = NewsOutletDtoFixtureBase.OutletDtos[1];

        _mockNewsOutlets = NewsOutletFixtureBase.Outlets[1];

        BasicSetup();

        _sut = new DeleteNewsOutletService(
            mockLogger.Object,
            _mockHelper.Object,
            _mockRepository.Object,
            _mockMapper.Object,
            _mockUnitOfWork.Object
        );
    }

    private readonly DeleteNewsOutletService _sut;
    private readonly Mock<INewsOutletServiceHelper> _mockHelper;
    private readonly Mock<INewsOutletRepository> _mockRepository;
    private readonly Mock<INewsOutletMapper> _mockMapper;
    private readonly Mock<IUnitOfWork<INneDbContext>> _mockUnitOfWork;
    private readonly List<NewsOutletDto> _mockNewsOutletDtos;
    private readonly List<NewsOutlet> _mockNewsOutlets;

    private void BasicSetup()
    {
        _mockMapper
            .Setup(mapper => mapper.DtosToEntities(_mockNewsOutletDtos))
            .Returns(_mockNewsOutlets);

        _mockMapper
            .Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(_mockNewsOutletDtos);

        _mockHelper
            .Setup(helper => helper.MatchNewsOutlets(_mockNewsOutlets))
            .ReturnsAsync((_mockNewsOutlets, new List<NewsOutlet>()));

        _mockRepository
            .Setup(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(true);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenEmptyListProvided_ReturnsError()
    {
        // Arrange
        var emptyDtoList = new List<NewsOutletDto>();

        // Act
        var result = await _sut.DeleteNewsOutlets(emptyDtoList);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NoNewsOutletProvided);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenMatchingFails_ReturnsError()
    {
        // Arrange
        _mockHelper
            .Setup(helper => helper.MatchNewsOutlets(_mockNewsOutlets))
            .ReturnsAsync(Errors.NewsOutlets.NotFoundInDb);

        // Act
        var result = await _sut.DeleteNewsOutlets(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.NotFoundInDb);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenNoMatchesFound_ReturnsUnmatchedDtos()
    {
        // Arrange
        _mockHelper
            .Setup(helper => helper.MatchNewsOutlets(_mockNewsOutlets))
            .ReturnsAsync((new List<NewsOutlet>(), _mockNewsOutlets));
        _mockMapper
            .Setup(mapper => mapper.EntitiesToDtos(It.IsAny<List<NewsOutlet>>()))
            .Returns(_mockNewsOutletDtos);

        // Act
        var result = await _sut.DeleteNewsOutlets(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_mockNewsOutletDtos);
        _mockRepository.Verify(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenDeletionFails_ReturnsError()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(false);

        // Act
        var result = await _sut.DeleteNewsOutlets(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.NewsOutlets.DeletionFailed);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenAllOutletsDeleted_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.DeleteNewsOutlets(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockRepository.Verify(repo => repo.RemoveRange(_mockNewsOutlets), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }

    [Fact]
    public async Task DeleteNewsOutlets_WhenPartiallyDeleted_ReturnsUnmatchedDtos()
    {
        // Arrange
        var matchedOutlet = _mockNewsOutlets[0];
        var unmatchedOutlet = _mockNewsOutlets[1];
        _mockHelper
            .Setup(helper => helper.MatchNewsOutlets(_mockNewsOutlets))
            .ReturnsAsync((new List<NewsOutlet> { matchedOutlet }, new List<NewsOutlet> { unmatchedOutlet }));
        _mockMapper.Setup(mapper =>
                mapper.EntitiesToDtos(new List<NewsOutlet> { unmatchedOutlet }))
            .Returns([_mockNewsOutletDtos[1]]);

        // Act
        var result = await _sut.DeleteNewsOutlets(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.Should().ContainEquivalentOf(_mockNewsOutletDtos[1]);
        _mockRepository.Verify(repo => repo.RemoveRange(It.Is<List<NewsOutlet>>(list => 
            list.Single().Id == matchedOutlet.Id)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }
}