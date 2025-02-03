using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dtr_nne.Application.DTO.Article;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class BaseNewsArticleDto : IManagedEntityDto
{
    public int Id { get; set; }
    public string Header { get; set; } = string.Empty;
    public string TranslatedHeader { get; set; } = string.Empty;
    [Required]
    public Uri? Uri { get; set; }
    
    public string Error { get; set; } = string.Empty;
}