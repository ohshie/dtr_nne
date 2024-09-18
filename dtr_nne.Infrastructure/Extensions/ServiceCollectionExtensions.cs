using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using dtr_nne.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dtr_nne.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastucture(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Db
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        serviceCollection.AddDbContext<NneDbContext>(s =>
        {
            s.UseSqlite(connectionString);
        });

        // Unit Of Work
        serviceCollection.AddScoped<IUnitOfWork<NneDbContext>, UnitOfWork<NneDbContext>>();
        
        // Repositories
        serviceCollection.AddTransient<INewsOutletRepository, NewsOutletRepository>();
    }
}