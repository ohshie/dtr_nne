using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;

namespace Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

public class OpenAiAssistantDtoFixture : TheoryData<List<OpenAiAssistantDto>>
{
    public OpenAiAssistantDtoFixture()
    {
        Add(OpenAiAssistantDtoFixtureBase.Assistants[0]);
        Add(OpenAiAssistantDtoFixtureBase.Assistants[1]);
    }
}