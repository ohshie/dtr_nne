using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class LlmApi
{
    public int Id { get; set; }
    [StringLength(50, MinimumLength = 0)]
    public string ApiKey { get; set; } = string.Empty;
}