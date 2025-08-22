using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Calculation> Calculations => Set<Calculation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Calculation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Expression).IsRequired().HasMaxLength(512);
            e.Property(x => x.Result).IsRequired().HasMaxLength(128);
            e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
