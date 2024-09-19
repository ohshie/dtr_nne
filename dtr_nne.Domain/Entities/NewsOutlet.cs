using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class NewsOutlet
{
    public int Id { get; set; }
    public bool InUse { get; set; }
    public bool AlwaysJs { get; set; }
    [StringLength(50, MinimumLength = 0)]
    public string Name { get; set; } = string.Empty;
    public Uri? Website { get; set; }
    [StringLength(300, MinimumLength = 0)]
    public string MainPagePassword { get; set; } = string.Empty;
    [StringLength(300, MinimumLength = 0)]
    public string NewsPassword { get; set; } = string.Empty;
}