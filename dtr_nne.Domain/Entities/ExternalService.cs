using System.ComponentModel.DataAnnotations;
using dtr_nne.Domain.Enums;

namespace dtr_nne.Domain.Entities;

public class ExternalService
{
    public int Id { get; set; }
    [StringLength(10, MinimumLength = 0)] 
    public string ServiceName { get; set; } = string.Empty;
    public ExternalServiceType Type { get; set; }
    public bool InUse { get; set; }
}