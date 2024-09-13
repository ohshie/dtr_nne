using System.Runtime.CompilerServices;
using dtr_nne.Domain.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Infrastructure.Context;

internal class NneDbContext(DbContextOptions<NneDbContext> options) : DbContext(options)
{
    internal DbSet<NewsOutlet> NewsOutlets { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}