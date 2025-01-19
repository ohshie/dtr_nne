using System.ComponentModel.DataAnnotations;

namespace dtr_nne.Domain.Entities;

public class EditedArticle
{
    public int Id { get; set; }
    
    [MaxLength(1000)]
    public string Header { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string HeaderRunId { get; set; } = string.Empty;
    [MaxLength(10000)]
    public string Subheader { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string SubheaderRunId { get; set; } = string.Empty;
    [MaxLength(30000)]
    public string EditedBody { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string EditedBodyRunId { get; set; } = string.Empty;
    [MaxLength(30000)]
    public string TranslatedBody { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string TranslatedBodyRunId { get; set; } = string.Empty;
}