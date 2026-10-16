using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Pgvector.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgvector.Tests;

public class ItemContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = "Host=localhost;Database=pgvector_dotnet_test";
        optionsBuilder.UseNpgsql(connString, o => o.UseVector());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Item>()
            .HasIndex(i => i.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_l2_ops")
            .HasStorageParameter("m", 16)
            .HasStorageParameter("ef_construction", 64);
    }
}

[Table("efcore_items")]
public class Item
{
    public int Id { get; set; }

    [Column("embedding", TypeName = "vector(3)")]
    public Vector? Embedding { get; set; }

    [Column("half_embedding", TypeName = "halfvec(3)")]
    public HalfVector? HalfEmbedding { get; set; }

    [Column("binary_embedding", TypeName = "bit(3)")]
    public BitArray? BinaryEmbedding { get; set; }

    [Column("sparse_embedding", TypeName = "sparsevec(3)")]
    public SparseVector? SparseEmbedding { get; set; }
}

public class EntityFrameworkCoreTests
{
    private class AutoLoadedItemContext : ItemContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Item>().Property(x => x.Embedding).Metadata.IsAutoLoaded = true;
            modelBuilder.Entity<Item>().Property(x => x.HalfEmbedding).Metadata.IsAutoLoaded = true;
            modelBuilder.Entity<Item>().Property(x => x.SparseEmbedding).Metadata.IsAutoLoaded = true;
        }
    }

    [Fact]
    public async Task Main()
    {
        await using var ctx = new AutoLoadedItemContext();

        ctx.Database.ExecuteSql($"DROP TABLE IF EXISTS efcore_items");
        var databaseCreator = ctx.GetService<IRelationalDatabaseCreator>();
        databaseCreator.CreateTables();

        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 1, 1, 1 }), HalfEmbedding = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)1 }), BinaryEmbedding = new BitArray(new bool[] { false, false, false }), SparseEmbedding = new SparseVector(new float[] { 1, 1, 1 }) });
        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 2, 2, 2 }), HalfEmbedding = new HalfVector(new Half[] { (Half)2, (Half)2, (Half)2 }), BinaryEmbedding = new BitArray(new bool[] { true, false, true }), SparseEmbedding = new SparseVector(new float[] { 2, 2, 2 }) });
        ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 1, 1, 2 }), HalfEmbedding = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)2 }), BinaryEmbedding = new BitArray(new bool[] { true, true, true }), SparseEmbedding = new SparseVector(new float[] { 1, 1, 2 }) });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = await ctx.Items.FromSql($"SELECT * FROM efcore_items ORDER BY embedding <-> {embedding} LIMIT 5").ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].Embedding!.ToArray());
        Assert.Equal(new Half[] { (Half)1, (Half)1, (Half)1 }, items[0].HalfEmbedding!.ToArray());
        Assert.Equal(new BitArray(new bool[] { false, false, false }), items[0].BinaryEmbedding!);
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].SparseEmbedding!.ToArray());

        // vector distance functions

        items = await ctx.Items.OrderBy(x => x.Embedding!.L2Distance(embedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].Embedding!.ToArray());

        items = await ctx.Items.OrderBy(x => x.Embedding!.MaxInnerProduct(embedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.Embedding!.CosineDistance(embedding)).Take(5).ToListAsync();
        Assert.Equal(3, items[2].Id);

        items = await ctx.Items.OrderBy(x => x.Embedding!.L1Distance(embedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());

        // halfvec distance functions

        var halfEmbedding = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)1 });
        items = await ctx.Items.OrderBy(x => x.HalfEmbedding!.L2Distance(halfEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.HalfEmbedding!.MaxInnerProduct(halfEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.HalfEmbedding!.CosineDistance(halfEmbedding)).Take(5).ToListAsync();
        Assert.Equal(3, items[2].Id);

        items = await ctx.Items.OrderBy(x => x.HalfEmbedding!.L1Distance(halfEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());

        // sparsevec distance functions

        var sparseEmbedding = new SparseVector(new float[] { 1, 1, 1 });
        items = await ctx.Items.OrderBy(x => x.SparseEmbedding!.L2Distance(sparseEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.SparseEmbedding!.MaxInnerProduct(sparseEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.SparseEmbedding!.CosineDistance(sparseEmbedding)).Take(5).ToListAsync();
        Assert.Equal(3, items[2].Id);

        items = await ctx.Items.OrderBy(x => x.SparseEmbedding!.L1Distance(sparseEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());

        // bit distance functions

        var binaryEmbedding = new BitArray(new bool[] { true, false, true });
        items = await ctx.Items.OrderBy(x => x.BinaryEmbedding!.HammingDistance(binaryEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        items = await ctx.Items.OrderBy(x => x.BinaryEmbedding!.JaccardDistance(binaryEmbedding)).Take(5).ToListAsync();
        Assert.Equal(new int[] { 2, 3, 1 }, items.Select(v => v.Id).ToArray());

        // additional

        items = await ctx.Items
            .OrderBy(x => x.Id)
            .Where(x => x.Embedding!.L2Distance(embedding) < 1.5)
            .ToListAsync();
        Assert.Equal(new int[] { 1, 3 }, items.Select(v => v.Id).ToArray());

        var neighbors = await ctx.Items
            .OrderBy(x => x.Embedding!.L2Distance(embedding))
            .Select(x => new { Entity = x, Distance = x.Embedding!.L2Distance(embedding) })
            .ToListAsync();
        Assert.Equal(new int[] { 1, 3, 2 }, neighbors.Select(v => v.Entity.Id).ToArray());
        Assert.Equal(new double[] { 0, 1, Math.Sqrt(3) }, neighbors.Select(v => v.Distance).ToArray());
    }

    [Fact]
    public async Task VectorPropertiesAreNotAutoLoaded()
    {
        await using var ctx = new ItemContext();
        var entityType = ctx.Model.FindEntityType(typeof(Item))!;
        Assert.False(entityType.FindProperty(nameof(Item.Embedding))!.IsAutoLoaded);
        Assert.False(entityType.FindProperty(nameof(Item.HalfEmbedding))!.IsAutoLoaded);
        Assert.False(entityType.FindProperty(nameof(Item.SparseEmbedding))!.IsAutoLoaded);
        Assert.True(entityType.FindProperty(nameof(Item.Id))!.IsAutoLoaded);
        Assert.True(entityType.FindProperty(nameof(Item.BinaryEmbedding))!.IsAutoLoaded);

        await ctx.Database.ExecuteSqlAsync($"DROP TABLE IF EXISTS efcore_items");
        ctx.GetService<IRelationalDatabaseCreator>().CreateTables();

        var embedding = new Vector(new float[] { 1, 2, 3 });
        var halfEmbedding = new HalfVector(new Half[] { (Half)1, (Half)2, (Half)3 });
        var sparseEmbedding = new SparseVector(new float[] { 1, 2, 3 });
        var binaryEmbedding = new BitArray(new bool[] { true, false, true });
        ctx.Items.Add(new Item
        {
            Embedding = embedding,
            HalfEmbedding = halfEmbedding,
            SparseEmbedding = sparseEmbedding,
            BinaryEmbedding = binaryEmbedding
        });
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var sql = ctx.Items.ToQueryString();
        Assert.DoesNotContain(".embedding", sql);
        Assert.DoesNotContain(".half_embedding", sql);
        Assert.DoesNotContain(".sparse_embedding", sql);
        Assert.Contains(".binary_embedding", sql);

        var untracked = await ctx.Items.AsNoTracking().SingleAsync();
        Assert.Null(untracked.Embedding);
        Assert.Null(untracked.HalfEmbedding);
        Assert.Null(untracked.SparseEmbedding);
        Assert.Equal(binaryEmbedding, untracked.BinaryEmbedding);

        var neighbor = await ctx.Items
            .Where(x => x.Embedding!.L2Distance(embedding) < 1)
            .OrderBy(x => x.Embedding!.L2Distance(embedding))
            .Select(x => new { Entity = x, Distance = x.Embedding!.L2Distance(embedding) })
            .SingleAsync();
        Assert.Equal(untracked.Id, neighbor.Entity.Id);
        Assert.Equal(0, neighbor.Distance);
        Assert.Null(neighbor.Entity.Embedding);
        Assert.Null(neighbor.Entity.HalfEmbedding);
        Assert.Null(neighbor.Entity.SparseEmbedding);
        Assert.Equal(binaryEmbedding, neighbor.Entity.BinaryEmbedding);

        neighbor.Entity.BinaryEmbedding = new BitArray(new bool[] { false, true, false });
        await ctx.SaveChangesAsync();

        var projected = await ctx.Items
            .Select(x => new { x.Embedding, x.HalfEmbedding, x.SparseEmbedding, x.BinaryEmbedding })
            .SingleAsync();
        Assert.Equal(embedding, projected.Embedding);
        Assert.Equal(halfEmbedding, projected.HalfEmbedding);
        Assert.Equal(sparseEmbedding, projected.SparseEmbedding);
        Assert.Equal(neighbor.Entity.BinaryEmbedding, projected.BinaryEmbedding);
    }

    [Theory]
    [InlineData(typeof(Vector), null, "vector")]
    [InlineData(typeof(Vector), 3, "vector(3)")]
    [InlineData(typeof(HalfVector), null, "halfvec")]
    [InlineData(typeof(HalfVector), 3, "halfvec(3)")]
    [InlineData(typeof(BitArray), null, "bit varying")]
    [InlineData(typeof(BitArray), 3, "bit varying(3)")]
    [InlineData(typeof(SparseVector), null, "sparsevec")]
    [InlineData(typeof(SparseVector), 3, "sparsevec(3)")]
    public void ByStoreType(Type type, int? size, string expectedStoreType)
    {
        using var ctx = new ItemContext();
        var typeMappingSource = ctx.GetService<IRelationalTypeMappingSource>();

        var typeMapping = typeMappingSource.FindMapping(type, storeTypeName: null, size: size)!;
        Assert.Equal(expectedStoreType, typeMapping.StoreType);
        Assert.Same(type, typeMapping.ClrType);
        Assert.Equal(size, typeMapping.Size);
    }

    [Theory]
    [InlineData("vector", typeof(Vector), null)]
    [InlineData("vector(3)", typeof(Vector), 3)]
    [InlineData("halfvec", typeof(HalfVector), null)]
    [InlineData("halfvec(3)", typeof(HalfVector), 3)]
    [InlineData("bit varying", typeof(BitArray), null)]
    [InlineData("bit(3)", typeof(BitArray), 3)]
    [InlineData("sparsevec", typeof(SparseVector), null)]
    [InlineData("sparsevec(3)", typeof(SparseVector), 3)]
    public void ByClrType(string storeType, Type expectedType, int? expectedSize)
    {
        using var ctx = new ItemContext();
        var typeMappingSource = ctx.GetService<IRelationalTypeMappingSource>();

        var typeMapping = typeMappingSource.FindMapping(storeType)!;
        Assert.Equal(storeType, typeMapping.StoreType);
        Assert.Same(expectedType, typeMapping.ClrType);
        Assert.Equal(expectedSize, typeMapping.Size);
    }
}
