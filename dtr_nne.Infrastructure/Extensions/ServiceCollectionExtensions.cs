using dtr_nne.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastucture(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        serviceCollection.AddDbContext<NneDbContext>(s =>
        {
            s.UseSqlite(connectionString);
        });
    }
}