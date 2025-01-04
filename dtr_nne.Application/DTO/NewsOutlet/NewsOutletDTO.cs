namespace dtr_nne.Application.DTO.NewsOutlet;

public class NewsOutletDto : BaseNewsOutletsDto
{
    public required bool InUse { get; set; }
    public required bool AlwaysJs { get; set; }
    public required Uri? Website { get; set; }
    public required string MainPagePassword { get; set; } = string.Empty;
    public required string NewsPassword { get; set; } = string.Empty;
}