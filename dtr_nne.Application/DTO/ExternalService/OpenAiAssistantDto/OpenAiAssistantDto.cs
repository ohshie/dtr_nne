using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;

public class OpenAiAssistantDto : IManagedEntityDto
{
    public int Id { get; set; }
    [StringLength(20)]
    public required string Role { get; set; }
    [StringLength(100)]
    public required string AssistantId { get; set; }
}