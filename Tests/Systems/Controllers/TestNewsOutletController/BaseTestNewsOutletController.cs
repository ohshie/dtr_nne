using dtr_nne.Application.Services.NewsOutletServices;
using dtr_nne.Controllers;
using Moq;

namespace Tests.Systems.Controllers.TestNewsOutletController;

public abstract class BaseTestNewsOutletController
{
    internal readonly Mock<IGetNewsOutletService> MockGetNewsOutletService;
    internal readonly Mock<IAddNewsOutletService> MockAddNewsOutletService;
    internal readonly Mock<IUpdateNewsOutletService> MockUpdateNewsOutletService;
    internal readonly Mock<IDeleteNewsOutletService> MockDeleteNewsOutletService;
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