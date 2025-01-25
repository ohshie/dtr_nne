using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Controllers;
using Moq;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public abstract class BaseTestNewsOutletController
{
    internal readonly Mock<IGetManagerEntity<NewsOutletDto>> MockGetNewsOutletService;
    internal readonly Mock<IAddManagedEntity<NewsOutletDto>> MockAddNewsOutletService;
    internal readonly Mock<IUpdateManagedEntity<NewsOutletDto>> MockUpdateNewsOutletService;
    internal readonly Mock<IDeleteManagedEntity<BaseNewsOutletsDto>> MockDeleteNewsOutletService;
    internal readonly NewsOutletController Sut;
    
    public BaseTestNewsOutletController()
    {
        MockGetNewsOutletService = new();
        MockAddNewsOutletService = new();
        MockUpdateNewsOutletService = new();
        MockDeleteNewsOutletService = new();
        
        Sut = new NewsOutletController(MockGetNewsOutletService.Object,
            MockAddNewsOutletService.Object, 
            MockUpdateNewsOutletService.Object, 
            MockDeleteNewsOutletService.Object);
    }
}