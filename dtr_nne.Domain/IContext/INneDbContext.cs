namespace dtr_nne.Domain.IContext;

public interface INneDbContext
{
    public Task EnsureCreatedAsync();

    public Task MigrateAsync();
}