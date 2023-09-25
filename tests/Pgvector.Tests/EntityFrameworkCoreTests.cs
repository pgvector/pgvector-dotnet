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

        modelBuilder.Entity<Item>()
            .HasIndex(i => i.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_l2_ops");
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
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].Embedding!.ToArray());

        items = await ctx.Items.OrderBy(x => x.Embedding!.L2Distance(embedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].Embedding!.ToArray());

        items = await ctx.Items.OrderBy(x => x.Embedding!.MaxInnerProduct(embedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.Embedding!.CosineDistance(embedding)).Take(5).ToListAsync();
        Assert.Equal(3, items[2].Id);
    }
}
