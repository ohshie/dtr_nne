namespace dtr_nne.Application.DTO.NewsOutlet;

public class NewsOutletDto : BaseNewsOutletsDto
{
    public required bool InUse { get; set; }
    public required bool AlwaysJs { get; set; }
    
    public string WaitTime { get; set; } = string.Empty;
    public required Uri Website { get; set; }
    public string MainPagePassword { get; set; } = string.Empty;
    public required string NewsPassword { get; set; }
    public required List<string> Themes { get; set; }
}