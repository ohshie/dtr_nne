using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Entities.ScrapableEntities;

namespace dtr_nne.Domain.Entities;

public class ArticleContent
{
    [Key]
    public int Id { get; set; }

    [DefaultValue("")]
    [MaxLength(30000)]
    public string Body { get; set; } = string.Empty;
    
    public List<Uri> Images { get; set; } = [];
    
    public List<string> Copyright { get; set; } = [];
    
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty;

    public int HeadlineId { get; set; }
    public Headline? Headline { get; set; }
    
    public int NewsArticleId { get; set; }
    public NewsArticle? NewsArticle { get; set; }
    
    public int EditedArticleId { get; set; }
    public EditedArticle? EditedArticle { get; set; }
}
