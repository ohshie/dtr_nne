using dtr_nne.Domain.Entities;
using ErrorOr;

namespace dtr_nne.Domain.ExternalServices;

public interface IOpenAiService : IExternalService, ILlmService
{
}