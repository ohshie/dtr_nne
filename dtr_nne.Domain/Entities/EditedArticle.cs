namespace dtr_nne.Domain.Entities;

public class EditedArticle
{
    public string Header { get; set; } = string.Empty;
    public string HeaderRunId { get; set; } = string.Empty;
    public string Subheader { get; set; } = string.Empty;
    public string SubheaderRunId { get; set; } = string.Empty;
    public string EditedBody { get; set; } = string.Empty;
    public string EditedBodyRunId { get; set; } = string.Empty;
    public string TranslatedBody { get; set; } = string.Empty;
    public string TranslatedBodyRunId { get; set; } = string.Empty;
}