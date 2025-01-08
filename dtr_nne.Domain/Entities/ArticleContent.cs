namespace dtr_nne.Domain.Entities;

public class ArticleContent
{
    public int Id { get; set; }
    public string OriginalBody { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Subheader { get; set; } = string.Empty;
    public Headline? Headline { get; set; }
    public EditedArticle? EditedArticle { get; set; }
}