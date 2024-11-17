using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Enums;

namespace dtr_nne.Application.DTO.ExternalService;

public class ExternalServiceDto : BaseExternalServiceDto
{
    public required ExternalServiceType Type { get; set; }
    public bool InUse { get; set; }
    [StringLength(100, MinimumLength = 0)]
    public string ApiKey { get; set; } = string.Empty;
}