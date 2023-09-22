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

public class EntityFrameworkCoreFixture : IDisposable
{
    public ItemContext Db { get; private set; }

    public EntityFrameworkCoreFixture()
    {
        var db = new ItemContext();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        for (int i = -5; i <= 5; i++)
        {
            db.Items.Add(new Item { Embedding = new Vector(new float[] { i, i, i }) });
        }

        db.SaveChanges();

        Db = db;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Db.Dispose();
    }
}

public class EntityFrameworkCoreTests : IClassFixture<EntityFrameworkCoreFixture>
{
    private readonly EntityFrameworkCoreFixture _fixture;

    public EntityFrameworkCoreTests(EntityFrameworkCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task L2DistanceSelectOrdered()
    {
        var db = _fixture.Db;

        var embedding = new Vector(new float[] { 1, 1, 1 });

        var itemsA = await db.Items
            .FromSql($"SELECT * FROM efcore_items ORDER BY embedding <-> {embedding} LIMIT 5")
            .ToListAsync();

        var itemsB = await db.Items
            .OrderBy(x => x.Embedding!.L2Distance(embedding))
            .Take(5)
            .ToListAsync();

        foreach (Item item in itemsB)
        {
            if (item.Embedding != null)
            {
                Console.WriteLine(item.Embedding);
            }
        }

        Assert.Equal(itemsA.Count, itemsB.Count); // Check amount
        Assert.Equal(itemsA.Select(x => x.Id).ToArray(), itemsB.Select(x => x.Id).ToArray()); // Check order
    }

    [Fact]
    public async Task CosineDistanceSelectOrdered()
    {
        var db = _fixture.Db;

        var embedding = new Vector(new float[] { 1, 1, 1 });

        var itemsA = await db.Items
            .FromSql($"SELECT * FROM efcore_items ORDER BY embedding <=> {embedding} LIMIT 5")
            .ToListAsync();

        var itemsB = await db.Items
            .OrderBy(x => x.Embedding!.CosineDistance(embedding))
            .Take(5)
            .ToListAsync();

        foreach (Item item in itemsB)
        {
            if (item.Embedding != null)
            {
                Console.WriteLine(item.Embedding);
            }
        }

        Assert.Equal(itemsA.Count, itemsB.Count); // Check amount
        Assert.Equal(itemsA.Select(x => x.Id).ToArray(), itemsB.Select(x => x.Id).ToArray()); // Check order
    }

    [Fact]
    public async Task MaxInnerProductSelectOrdered()
    {
        var db = _fixture.Db;

        var embedding = new Vector(new float[] { 1, 1, 1 });

        var itemsA = await db.Items
            .FromSql($"SELECT * FROM efcore_items ORDER BY embedding <#> {embedding} LIMIT 5")
            .ToListAsync();

        var itemsB = await db.Items
            .OrderBy(x => x.Embedding!.MaxInnerProduct(embedding))
            .Take(5)
            .ToListAsync();

        foreach (Item item in itemsB)
        {
            if (item.Embedding != null)
            {
                Console.WriteLine(item.Embedding);
            }
        }

        Assert.Equal(itemsA.Count, itemsB.Count); // Check amount
        Assert.Equal(itemsA.Select(x => x.Id).ToArray(), itemsB.Select(x => x.Id).ToArray()); // Check order
    }
}
