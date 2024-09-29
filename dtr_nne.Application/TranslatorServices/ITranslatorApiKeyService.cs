using dtr_nne.Application.DTO.Translator;

namespace dtr_nne.Application.TranslatorServices;

public interface ITranslatorApiKeyService
{
    public Task<ErrorOr<TranslatorApiDto>> Add(TranslatorApiDto apiKey);
    public Task<ErrorOr<TranslatorApiDto>> UpdateKey(TranslatorApiDto apiKey);
}