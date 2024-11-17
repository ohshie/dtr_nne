namespace dtr_nne.Application.DTO.ExternalService;

public class BaseExternalServiceDto
{
    public virtual int Id { get; set; }
    public virtual required string ServiceName { get; set; } = string.Empty;
}