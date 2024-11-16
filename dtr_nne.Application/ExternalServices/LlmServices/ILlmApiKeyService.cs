using dtr_nne.Application.DTO.Llm;

namespace dtr_nne.Application.ExternalServices.LlmServices;

public interface ILlmApiKeyService
{
    Task<ErrorOr<LlmApiDto>> Add(LlmApiDto apiKey);
    Task<ErrorOr<LlmApiDto>> UpdateKey(LlmApiDto apiKey);
}