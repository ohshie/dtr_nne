using System.Runtime.CompilerServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Infrastructure.Context;

internal class NneDbContext(DbContextOptions<NneDbContext> options) : DbContext(options), INneDbContext
{
    internal DbSet<NewsOutlet> NewsOutlets { get; set; }
    internal DbSet<NewsArticle> NewsArticles { get; set; }
    internal DbSet<ExternalService> ExternalServices { get; set; }
    internal DbSet<OpenAiAssistant> OpenAiAssistants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsOutlet>()
            .HasMany(no => no.Articles)
            .WithOne(na => na.NewsOutlet)
            .HasForeignKey(na => na.NewsOutletId)
            .IsRequired();
    }
    
    public async Task EnsureCreatedAsync()
    {
        await Database.EnsureCreatedAsync();
    }

    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }
}