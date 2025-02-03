namespace dtr_nne.Application.DTO.NewsOutlet;

public class BaseNewsOutletsDto : IManagedEntityDto
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
}