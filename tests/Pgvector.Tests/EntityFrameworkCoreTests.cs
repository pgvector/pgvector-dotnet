using Xunit;
using Microsoft.EntityFrameworkCore;
using Pgvector.Npgsql;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgvector.Tests;

public class ItemContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = "Host=localhost;Database=pgvector_dotnet_test";
        optionsBuilder.UseNpgsql(connString).UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        // TODO set lists = 1
        // modelBuilder.Entity<Item>()
        //     .HasIndex(i => i.Embedding)
        //     .HasMethod("ivfflat")
        //     .HasOperators("vector_l2_ops");
    }
}

[Table("efcore_items"), Keyless]
public class Item
{
    [Column(TypeName = "vector(3)")]
    public string? Embedding { get; set; }
}

public class EntityFrameworkCoreTests
{
    [Fact]
    public async Task Main()
    {
        await using var ctx = new ItemContext();
        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.EnsureCreatedAsync();

        var embedding1 = new Vector(new float[] { 1, 1, 1 });
        var embedding2 = new Vector(new float[] { 2, 2, 2 });
        var embedding3 = new Vector(new float[] { 1, 1, 2 });
        ctx.Database.ExecuteSql($"INSERT INTO efcore_items (embedding) VALUES ({embedding1.ToString()}::vector), ({embedding2.ToString()}::vector), ({embedding3.ToString()}::vector)");

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = await ctx.Items.FromSql($"SELECT embedding::text FROM efcore_items ORDER BY embedding <-> {embedding.ToString()}::vector LIMIT 5").ToListAsync();
        foreach (Item item in items)
        {
            if (item.Embedding != null)
            {
                Console.WriteLine(new Vector(item.Embedding));
            }
        }
    }
}
