using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Application.DTO.Article;

public class ArticleDto
{
    public string OriginalBody { get; set; } = string.Empty;
    [Required]
    public string Body { get; set; } = string.Empty;
    public string Subheader { get; set; } = string.Empty;
    public string DzenHeader { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
}