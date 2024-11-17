using dtr_nne.Application.DTO.ExternalService;
using dtr_nne.Application.DTO.Translator;

namespace dtr_nne.Application.ExternalServices.TranslatorServices;

public interface ITranslatorApiKeyService
{
    public Task<ErrorOr<ExternalServiceDto>> Add(ExternalServiceDto apiKey);
    public Task<ErrorOr<ExternalServiceDto>> UpdateKey(ExternalServiceDto apiKey);
}