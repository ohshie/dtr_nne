namespace dtr_nne.Domain.Entities;

public class InternalAiAssistant
{
    public string ApiKey { get; set; } = string.Empty;
    public List<OpenAiAssistant> Assistants { get; set; } = new();
}