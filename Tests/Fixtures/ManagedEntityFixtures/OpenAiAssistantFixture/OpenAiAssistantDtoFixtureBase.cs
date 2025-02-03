using Bogus;
using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;

namespace Tests.Fixtures.ManagedEntityFixtures.OpenAiAssistantFixture;

public class OpenAiAssistantDtoFixtureBase
{
    private static readonly Faker Faker = new();

    public static readonly List<List<OpenAiAssistantDto>> Assistants =
    [
        [
            new OpenAiAssistantDto()
            {
                AssistantId = Faker.Random.Guid().ToString(),
                Role = Faker.Lorem.Word()
            }
        ],
        [
            new OpenAiAssistantDto()
            {
                AssistantId = Faker.Random.Guid().ToString(),
                Role = Faker.Lorem.Word()
            },
            new OpenAiAssistantDto()
            {
                AssistantId = Faker.Random.Guid().ToString(),
                Role = Faker.Lorem.Word()
            },
            new OpenAiAssistantDto()
            {
                AssistantId = Faker.Random.Guid().ToString(),
                Role = Faker.Lorem.Word()
            }
        ]
    ];
}