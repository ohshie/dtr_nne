using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Domain.Entities.ManagedEntities;

public class NewsOutlet : IManagedEntity, IScrapableEntity
{
    public int Id { get; set; }
    public bool InUse { get; set; }
    public bool AlwaysJs { get; set; }
    [StringLength(50, MinimumLength = 0)] public required string Name { get; set; } = string.Empty;

    public required Uri? Website { get; set; }
    
    [StringLength(300, MinimumLength = 0)]
    public required string MainPagePassword { get; set; } = string.Empty;
    [StringLength(300, MinimumLength = 0)]
    public required string NewsPassword { get; set; } = string.Empty;
    [StringLength(10, MinimumLength = 0)]
    public string WaitTimer { get; set; } = string.Empty;
    public required List<string> Themes { get; set; } = [];
    public List<NewsArticle>? Articles { get; set; }
}