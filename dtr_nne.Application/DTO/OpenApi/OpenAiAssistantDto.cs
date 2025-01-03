using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Application.DTO.OpenApi;

public class OpenAiAssistantDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
    [Required]
    public string AssistantId { get; set; } = string.Empty;
}