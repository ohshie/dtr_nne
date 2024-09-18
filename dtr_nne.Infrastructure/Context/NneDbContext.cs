using System.Runtime.CompilerServices;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Infrastructure.Context;

internal class NneDbContext(DbContextOptions<NneDbContext> options) : DbContext(options), INneDbContext
{
    internal DbSet<NewsOutlet> NewsOutlets { get; set; }
}