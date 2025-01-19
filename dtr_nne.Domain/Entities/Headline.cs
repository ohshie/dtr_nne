using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class Headline
{
    public int Id { get; set; }
    [MaxLength(1000)]
    public string OriginalHeadline { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string TranslatedHeadline { get; set; } = string.Empty;
}