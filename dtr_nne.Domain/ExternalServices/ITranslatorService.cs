using dtr_nne.Domain.Entities;
using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface ITranslatorService
{
    public Task<ErrorOr<List<Headline>>> Translate(List<Headline> headlines, TranslatorApi? translatorApi = null);
}