using dtr_nne.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace dtr_nne.Infrastructure.Context;

internal class NneDbContext(DbContextOptions<NneDbContext> options) : DbContext(options)
{
    internal DbSet<NewsOutlet> NewsOutlets { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}