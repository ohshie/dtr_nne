namespace dtr_nne.Application.DTO.ExternalService;

public class BaseExternalServiceDto
{
    public virtual int Id { get; set; }
    public virtual string ServiceName { get; set; } = string.Empty;
}