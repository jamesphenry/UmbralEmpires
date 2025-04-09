using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for optional logging
using System.Text.Json; // Added for JSON serialization
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;

namespace UmbralEmpires.Data;

public class UmbralDbContext : DbContext
{
    // --- DbSets ---
    // Define properties for EF Core to track our entity sets
    public DbSet<Astro> Astros { get; set; } = null!;
    public DbSet<Base> Bases { get; set; } = null!;
    // We'll add DbSet<Player> later when needed

    // --- Configuration ---
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use the Sqlite provider and specify the database filename.
        // This file will typically be created in the output directory of the running application.
        optionsBuilder.UseSqlite("Data Source=UmbralEmpires.db");

        // Optional: Add console logging to see EF Core commands (useful for debugging)
        // optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        // optionsBuilder.EnableSensitiveDataLogging(); // Development only!
    }

    // --- Model Configuration (Fluent API) ---
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- Astro Entity Configuration -----
        modelBuilder.Entity<Astro>(entity =>
        {
            entity.HasKey(a => a.Id); // Primary Key

            // Configure the AstroCoordinates value object as an owned entity type
            // This maps the record's properties to columns on the Astro table
            entity.OwnsOne(a => a.Coordinates);

            // Store the TerrainType enum as a string in the database (more readable)
            entity.Property(a => a.Terrain).HasConversion<string>();

            // Ensure BaseId is unique if set (an Astro can only have one Base)
            entity.HasIndex(a => a.BaseId).IsUnique();
        });

        // ----- Base Entity Configuration -----
        modelBuilder.Entity<Base>(entity =>
        {
            entity.HasKey(b => b.Id); // Primary Key
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100); // Example constraint

            // Define the one-to-one relationship between Base and Astro
            entity.HasOne<Astro>()              // A Base is related to one Astro...
                  .WithOne()                   // ...and an Astro relates to one Base (no nav prop needed on Astro side)
                  .HasForeignKey<Base>(b => b.AstroId); // ...using AstroId on Base as the FK.

            // Store the dictionary of structures as a JSON string column
            // Requires System.Text.Json
            var jsonOptions = new JsonSerializerOptions(); // Use default options
            entity.Property(b => b.Structures)
                  .HasConversion(
                      // Convert Dictionary -> string
                      dict => JsonSerializer.Serialize(dict, jsonOptions),
                      // Convert string -> Dictionary
                      json => JsonSerializer.Deserialize<Dictionary<StructureType, int>>(json, jsonOptions) ?? new Dictionary<StructureType, int>())
                  // Use a ValueComparer if EF Core has trouble detecting changes inside the dictionary
                  .Metadata.SetValueComparer(ValueComparerFactory.Create<Dictionary<StructureType, int>>(true));


            // Store the list of queue items also as a JSON string column
            entity.Property(b => b.ConstructionQueue)
                 .HasConversion(
                     // Convert List -> string
                     list => JsonSerializer.Serialize(list, jsonOptions),
                     // Convert string -> List
                     json => JsonSerializer.Deserialize<List<ConstructionQueueItem>>(json, jsonOptions) ?? new List<ConstructionQueueItem>())
                 // Use a ValueComparer for the list as well
                 .Metadata.SetValueComparer(ValueComparerFactory.Create<List<ConstructionQueueItem>>(true));


            entity.HasIndex(b => b.PlayerId); // Index for potentially faster lookups
        });

        // Tell EF Core that StructureType enum should generally be stored as string
        // This helps with the JSON serialization/deserialization
        modelBuilder.Entity<Base>().Property(e => e.Structures).Metadata
            .GetValueConverter()?.ProviderClrType.GetGenericArguments().FirstOrDefault()?
            .GetGenericArguments().FirstOrDefault()?.IsEnum = true; // Hint for enum handling? Might not be needed with default JSON options. Alternatively configure JsonSerializerOptions.


        // NOTE: Complex types stored as JSON might have limitations with querying directly into the JSON structure.
        // For simple storage and retrieval like this, it's often sufficient.
    }
}