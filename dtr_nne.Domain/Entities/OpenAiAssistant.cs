using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class OpenAiAssistant
{
    public int Id { get; set; }
    [StringLength(20)]
    public string Role { get; set; } = string.Empty;
    [StringLength(100)]
    public string AssistantId { get; set; } = string.Empty;
}