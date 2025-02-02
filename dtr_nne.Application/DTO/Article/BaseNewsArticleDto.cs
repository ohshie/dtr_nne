using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Application.DTO.Article;

public class BaseNewsArticleDto : IManagedEntityDto
{
    public int Id { get; set; }
    public string OriginalHeadline { get; set; } = string.Empty;
    public string TranslatedHeadline { get; set; } = string.Empty;
    [Required]
    public Uri? Uri { get; set; }
    public string Error { get; set; } = string.Empty;
}