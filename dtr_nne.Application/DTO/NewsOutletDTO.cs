namespace dtr_nne.Application.DTO;

public class NewsOutletDto
{
    public required int Id { get; set; }
    public bool InUse { get; set; }
    public bool AlwaysJs { get; set; }
    public required string Name { get; set; } = string.Empty;
    public Uri? Website { get; set; }
    public string MainPagePassword { get; set; } = string.Empty;
    public string NewsPassword { get; set; } = string.Empty;
}