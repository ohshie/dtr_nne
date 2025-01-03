namespace dtr_nne.Domain.Entities;

public class Article
{
    public string OriginalBody { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Subheader { get; set; } = string.Empty;
    public Headline? Headline { get; set; }
    public EditedArticle? EditedArticle { get; set; }
}