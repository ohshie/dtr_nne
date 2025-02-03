using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Application.DTO.ExternalService;

public class ExternalServiceDto : BaseExternalServiceDto
{
    public bool InUse { get; set; }
    [StringLength(200, MinimumLength = 0)]
    public string ApiKey { get; set; } = string.Empty;
}