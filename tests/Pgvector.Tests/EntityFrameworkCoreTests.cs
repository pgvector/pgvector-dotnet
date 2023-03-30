using Xunit;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgvector.Tests;

public class ItemContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = "Host=localhost;Database=pgvector_dotnet_test";
        optionsBuilder.UseNpgsql(connString, o => o.UseVector()).UseSnakeCaseNamingConvention();
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

[Table("efcore_items")]
public class Item
{
    public int Id { get; set; }

    [Column(TypeName = "vector(3)")]
    public Vector? Embedding { get; set; }
}

public class EntityFrameworkCoreTests
{
    [Fact]
    public async Task Main()
    {
        await using var ctx = new ItemContext();
        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.EnsureCreatedAsync();

        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 1, 1, 1 }) });
        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 2, 2, 2 }) });
        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 1, 1, 2 }) });
        ctx.SaveChanges();

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = await ctx.Items.FromSql($"SELECT * FROM efcore_items ORDER BY embedding <-> {embedding} LIMIT 5").ToListAsync();
        foreach (Item item in items)
        {
            if (item.Embedding != null)
            {
                Console.WriteLine(item.Embedding);
            }
        }
    }
}
