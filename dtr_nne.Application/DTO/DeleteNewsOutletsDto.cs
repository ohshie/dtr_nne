namespace dtr_nne.Application.DTO;

public class DeleteNewsOutletsDto : BaseNewsOutletsDto
{
    public override required int Id { get; set; }
    public override required string Name { get; set; }
}