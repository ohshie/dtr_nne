using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class NewsArticle
{
    [Key]
    public int Id { get; set; }

    public List<string> Themes { get; set; } = [];
    public Uri? Uri { get; set; }
    public string Error { get; set; } = string.Empty;
    public DateTime ParseTime { get; set; }
    
    public int NewsOutletId { get; set; }
    public NewsOutlet? NewsOutlet { get; set; }
    
    public ArticleContent ArticleContent { get; set; } = new();
}