using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletDtoFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestManagedEntityService;

public class TestManagedEntityService
{
    public TestManagedEntityService()
    {
        _mockHelper = new Mock<IManagedEntityHelper<NewsOutlet>>();
        _mockRepository = new Mock<IRepository<NewsOutlet>>();
        _mockMapper = new Mock<IManagedEntityMapper>();
        _mockUnitOfWork = new Mock<IUnitOfWork<INneDbContext>>();

        _mockNewsOutletDtos = NewsOutletDtoFixtureBase.OutletDtos[1];

        _mockNewsOutlets = NewsOutletFixtureBase.Outlets[1];

        BasicSetup();

        _sut = new DeleteManagedEntity<NewsOutlet, NewsOutletDto>(
            new Mock<ILogger<DeleteManagedEntity<NewsOutlet, NewsOutletDto>>>().Object,
            _mockMapper.Object,
            _mockHelper.Object,
            _mockRepository.Object,
            _mockUnitOfWork.Object
        );
    }

    private readonly DeleteManagedEntity<NewsOutlet, NewsOutletDto> _sut;
    private readonly Mock<IManagedEntityHelper<NewsOutlet>> _mockHelper;
    private readonly Mock<IRepository<NewsOutlet>> _mockRepository;
    private readonly Mock<IManagedEntityMapper> _mockMapper;
    private readonly Mock<IUnitOfWork<INneDbContext>> _mockUnitOfWork;
    private readonly List<NewsOutletDto> _mockNewsOutletDtos;
    private readonly List<NewsOutlet> _mockNewsOutlets;

    private void BasicSetup()
    {
        _mockMapper
            .Setup(mapper => mapper.DtoToEntity<NewsOutlet, NewsOutletDto>(_mockNewsOutletDtos))
            .Returns(_mockNewsOutlets);

        _mockMapper
            .Setup(mapper => mapper.EntityToDto<NewsOutlet, NewsOutletDto>(It.IsAny<List<NewsOutlet>>()))
            .Returns(_mockNewsOutletDtos);

        _mockHelper
            .Setup(helper => helper.MatchEntities(_mockNewsOutlets))
            .ReturnsAsync((_mockNewsOutlets, []));

        _mockRepository
            .Setup(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(true);
    }

    [Fact]
    public async Task Delete_WhenEmptyListProvided_ReturnsError()
    {
        // Arrange
        var emptyDtoList = new List<NewsOutletDto>();

        // Act
        var result = await _sut.Delete(emptyDtoList);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ManagedEntities.NoEntitiesProvided);
    }

    [Fact]
    public async Task Delete_WhenMatchingFails_ReturnsError()
    {
        // Arrange
        _mockHelper
            .Setup(helper => helper.MatchEntities(_mockNewsOutlets))
            .ReturnsAsync(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));

        // Act
        var result = await _sut.Delete(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }

    [Fact]
    public async Task Delete_WhenNoMatchesFound_ReturnsUnmatchedDtos()
    {
        // Arrange
        _mockHelper
            .Setup(helper => helper.MatchEntities(_mockNewsOutlets))
            .ReturnsAsync((new List<NewsOutlet>(){Capacity = 0}, _mockNewsOutlets));
        _mockMapper
            .Setup(mapper => mapper.EntityToDto<NewsOutlet, NewsOutletDto>(It.IsAny<List<NewsOutlet>>()))
            .Returns(_mockNewsOutletDtos);

        // Act
        var result = await _sut.Delete(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_mockNewsOutletDtos);
        _mockRepository.Verify(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenDeletionFails_ReturnsError()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.RemoveRange(It.IsAny<List<NewsOutlet>>()))
            .Returns(false);

        // Act
        var result = await _sut.Delete(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(Errors.ManagedEntities.DeletionFailed(typeof(NewsOutlet)));
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenAllOutletsDeleted_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.Delete(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockRepository.Verify(repo => repo.RemoveRange(_mockNewsOutlets), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenPartiallyDeleted_ReturnsUnmatchedDtos()
    {
        // Arrange
        var matchedOutlet = _mockNewsOutlets[0];
        var unmatchedOutlet = _mockNewsOutlets[1];
        
        _mockHelper
            .Setup(helper => helper.MatchEntities(_mockNewsOutlets))
            .ReturnsAsync(([matchedOutlet], [unmatchedOutlet]));
        _mockMapper.Setup(mapper =>
                mapper.EntityToDto<NewsOutlet, NewsOutletDto>(new List<NewsOutlet> { unmatchedOutlet }))
            .Returns([_mockNewsOutletDtos[1]]);

        // Act
        var result = await _sut.Delete(_mockNewsOutletDtos);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.Should().ContainEquivalentOf(_mockNewsOutletDtos[1]);
        _mockRepository.Verify(repo => repo.RemoveRange(It.Is<List<NewsOutlet>>(list => 
            list.Single().Id == matchedOutlet.Id)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }
}