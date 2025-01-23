using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Entities.ManagedEntities;

namespace dtr_nne.Domain.Entities;

public class NewsArticle
{
    [Key]
    public int Id { get; set; }

    public List<string> Themes { get; set; } = [];
    public Uri? Uri { get; set; }
    [MaxLength(10000)]
    public string Error { get; set; } = string.Empty;
    public DateTime ParseTime { get; set; }
    
    public int NewsOutletId { get; set; }
    public NewsOutlet? NewsOutlet { get; set; }

    public ArticleContent? ArticleContent { get; set; } = new();
    public EditedArticle? EditedArticle { get; set; } = new();
}