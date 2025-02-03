using System.Diagnostics.CodeAnalysis;

namespace dtr_nne.Application.DTO.Article;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class NewsArticleDto : BaseNewsArticleDto
{
    public string Body { get; set; } = string.Empty;
    public List<string> Themes { get; set; } = [];
    public List<Uri> Pictures { get; set; } = [];
    public List<string> Copyrights { get; set; } = [];
    public string Source { get; set; } = string.Empty;
}