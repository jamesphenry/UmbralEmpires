using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Still needed for ValueComparer<T> itself
using Microsoft.Extensions.Logging;
using System.Linq; // Needed for SequenceEqual, Aggregate, Except, Any
using System.Text.Json;
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;

namespace UmbralEmpires.Data;

public class UmbralDbContext : DbContext
{
    // --- DbSets ---
    public DbSet<Astro> Astros { get; set; } = null!;
    public DbSet<Base> Bases { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;

    // --- Configuration ---
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=UmbralEmpires.db");
    }

    // --- Model Configuration (Fluent API) ---
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- Astro Entity Configuration -----
        modelBuilder.Entity<Astro>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.OwnsOne(a => a.Coordinates);
            entity.Property(a => a.Terrain).HasConversion<string>();
            entity.HasIndex(a => a.BaseId).IsUnique();
        });

        // ----- Base Entity Configuration -----
        modelBuilder.Entity<Base>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.HasOne<Astro>()
                  .WithOne()
                  .HasForeignKey<Base>(b => b.AstroId);

            var jsonOptions = new JsonSerializerOptions();

            // Configure Dictionary<StructureType, int> as JSON string WITH direct ValueComparer
            entity.Property(b => b.Structures)
                  .HasConversion(
                      dict => JsonSerializer.Serialize(dict, jsonOptions),
                      json => JsonSerializer.Deserialize<Dictionary<StructureType, int>>(json, jsonOptions) ?? new Dictionary<StructureType, int>())
                  .Metadata.SetValueComparer(new ValueComparer<Dictionary<StructureType, int>>( // Direct Instantiation
                      (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.Count == c2.Count && !c1.Except(c2).Any()),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                      c => new Dictionary<StructureType, int>(c)
                  ));

            // Configure List<ConstructionQueueItem> as JSON string WITH direct ValueComparer
            entity.Property(b => b.ConstructionQueue)
                  .HasConversion(
                      list => JsonSerializer.Serialize(list, jsonOptions),
                      json => JsonSerializer.Deserialize<List<ConstructionQueueItem>>(json, jsonOptions) ?? new List<ConstructionQueueItem>())
                   .Metadata.SetValueComparer(new ValueComparer<List<ConstructionQueueItem>>( // Direct Instantiation from Reddit example
                       (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                       c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                       c => c.ToList()
                   ));

            entity.HasIndex(b => b.PlayerId);
        });

        // ----- Player Entity Configuration -----
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
            // Credits property maps automatically by convention
        });
    }
}