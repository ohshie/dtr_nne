using System.Text.Json.Serialization;
using dtr_nne.Domain.Enums;

namespace dtr_nne.Application.DTO.ExternalService;

public class BaseExternalServiceDto
{
    public int Id { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ExternalServiceType Type { get; set; }
    public required string ServiceName { get; set; } = string.Empty;
}