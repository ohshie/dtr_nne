namespace dtr_nne.Domain.Entities;

public class Headline
{
    public int Id { get; set; }
    public string OriginalHeadline { get; set; } = string.Empty;
    public string TranslatedHeadline { get; set; } = string.Empty;
}