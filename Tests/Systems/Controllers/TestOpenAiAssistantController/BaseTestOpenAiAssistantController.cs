using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Controllers;
using Moq;

namespace Tests.Systems.Controllers.TestOpenAiAssistantController;

public abstract class BaseTestOpenAiAssistantController
{
    internal readonly Mock<IGetManagedEntity<OpenAiAssistantDto>> MockGetOpenAiAssistantService;
    internal readonly Mock<IAddManagedEntity<OpenAiAssistantDto>> MockAddOpenAiAssistantService;
    internal readonly Mock<IUpdateManagedEntity<OpenAiAssistantDto>> MockUpdateOpenAiAssistantService;
    internal readonly Mock<IDeleteManagedEntity<OpenAiAssistantDto>> MockDeleteOpenAiAssistantService;
    internal readonly OpenAiAssistantController Sut;
    
    public BaseTestOpenAiAssistantController()
    {
        MockGetOpenAiAssistantService = new();
        MockAddOpenAiAssistantService = new();
        MockUpdateOpenAiAssistantService = new();
        MockDeleteOpenAiAssistantService = new();
        
        Sut = new OpenAiAssistantController(MockGetOpenAiAssistantService.Object,
            MockAddOpenAiAssistantService.Object, 
            MockUpdateOpenAiAssistantService.Object, 
            MockDeleteOpenAiAssistantService.Object);
    }
}