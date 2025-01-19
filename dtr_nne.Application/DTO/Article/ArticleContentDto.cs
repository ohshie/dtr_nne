using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.DTO.Article;

public class ArticleContentDto
{
    [Required]
    public string Body { get; set; } = string.Empty;
    public List<Uri>? Images { get; set; }
    public List<string> Copyright { get; set; } = [];
    public string Source { get; set; } = string.Empty;
    public Headline Headline { get; set; } = new();
}