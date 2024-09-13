namespace dtr_nne.Domain.Entities;

public class NewsOutlet
{
    public int Id { get; set; }
    public bool InUse { get; set; }
    public bool AlwaysJs { get; set; }
    public string Name { get; set; } = string.Empty;
    public Uri? Website { get; set; }
    public string MainPagePassword { get; set; } = string.Empty;
    public string NewsPassword { get; set; } = string.Empty;
}