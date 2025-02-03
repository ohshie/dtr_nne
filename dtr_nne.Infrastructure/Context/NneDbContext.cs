using System.Runtime.CompilerServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Entities.ScrapableEntities;
using dtr_nne.Domain.IContext;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("dtr_nne.API")]

namespace dtr_nne.Infrastructure.Context;

internal class NneDbContext(DbContextOptions<NneDbContext> options) : DbContext(options), INneDbContext
{
    internal DbSet<NewsOutlet> NewsOutlets { get; set; }
    internal DbSet<NewsArticle> NewsArticles { get; set; }
    internal DbSet<ArticleContent> ArticlesContent { get; set; }
    internal DbSet<EditedArticle> EditedArticles { get; set; }
    internal DbSet<Headline> Headlines { get; set; }
    internal DbSet<ExternalService> ExternalServices { get; set; }
    internal DbSet<OpenAiAssistant> OpenAiAssistants { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsOutlet>()
            .HasMany(no => no.Articles)
            .WithOne(na => na.NewsOutlet)
            .HasForeignKey(na => na.NewsOutletId)
            .IsRequired();
        
        modelBuilder.Entity<NewsArticle>()
            .HasOne(na => na.ArticleContent)
            .WithOne(ac => ac.NewsArticle)
            .HasForeignKey<ArticleContent>(ac => ac.NewsArticleId)
            .IsRequired();
        
        modelBuilder.Entity<ArticleContent>()
            .HasOne(ac => ac.Headline)
            .WithOne(h => h.ArticleContent)
            .HasForeignKey<Headline>(h => h.Id)
            .IsRequired();
        
        modelBuilder.Entity<ArticleContent>()
            .HasOne(ac => ac.EditedArticle)
            .WithOne(ea => ea.ArticleContent)
            .HasForeignKey<EditedArticle>(ea => ea.ArticleContentId)
            .IsRequired();
    }
    
    public async Task EnsureCreatedAsync()
    {
        await Database.EnsureCreatedAsync();
    }

    public async Task MigrateAsync()
    {
        foreach (var pending in await Database.GetPendingMigrationsAsync())
        {
            Console.WriteLine(pending);
        }
        await Database.MigrateAsync();
    }
}