using Dapper;
using Pgvector.Dapper;
using System.Collections;

namespace Pgvector.Tests;

public class DapperItem
{
    public int Id { get; set; }
    public Vector? Embedding { get; set; }
    public HalfVector? HalfEmbedding { get; set; }
    public BitArray? BinaryEmbedding { get; set; }
    public SparseVector? SparseEmbedding { get; set; }
}

public class DapperTests
{
    [Fact]
    public async Task Main()
    {
        SqlMapper.AddTypeHandler(new VectorTypeHandler());
        SqlMapper.AddTypeHandler(new HalfvecTypeHandler());
        SqlMapper.AddTypeHandler(new SparsevecTypeHandler());

        var connString = "Host=localhost;Database=pgvector_dotnet_test";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        conn.Execute("CREATE EXTENSION IF NOT EXISTS vector");
        conn.ReloadTypes();

        conn.Execute("DROP TABLE IF EXISTS dapper_items");
        conn.Execute("CREATE TABLE dapper_items (id serial PRIMARY KEY, embedding vector(3), halfembedding halfvec(3), binaryembedding bit(3), sparseembedding sparsevec(3))");

        var embedding1 = new Vector(new float[] { 1, 1, 1 });
        var embedding2 = new Vector(new float[] { 2, 2, 2 });
        var embedding3 = new Vector(new float[] { 1, 1, 2 });
        var halfEmbedding1 = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)1 });
        var halfEmbedding2 = new HalfVector(new Half[] { (Half)2, (Half)2, (Half)2 });
        var halfEmbedding3 = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)2 });
        var binaryEmbedding1 = new BitArray(new bool[] { false, false, false });
        var binaryEmbedding2 = new BitArray(new bool[] { true, false, true });
        var binaryEmbedding3 = new BitArray(new bool[] { true, true, true });
        var sparseEmbedding1 = new SparseVector(new float[] { 1, 1, 1 });
        var sparseEmbedding2 = new SparseVector(new float[] { 2, 2, 2 });
        var sparseEmbedding3 = new SparseVector(new float[] { 1, 1, 2 });
        conn.Execute(@"INSERT INTO dapper_items (embedding, halfembedding, binaryembedding, sparseembedding) VALUES (@embedding1, @halfEmbedding1, @binaryEmbedding1, @sparseEmbedding1), (@embedding2, @halfEmbedding2, @binaryEmbedding2, @sparseEmbedding2), (@embedding3, @halfEmbedding3, @binaryEmbedding3, @sparseEmbedding3)", new { embedding1, halfEmbedding1, binaryEmbedding1, sparseEmbedding1, embedding2, halfEmbedding2, binaryEmbedding2, sparseEmbedding2, embedding3, halfEmbedding3, binaryEmbedding3, sparseEmbedding3 });

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = conn.Query<DapperItem>("SELECT * FROM dapper_items ORDER BY embedding <-> @embedding LIMIT 5", new { embedding }).AsList();
        Assert.Equal(new int[] { 1, 3, 2 }, items.Select(v => v.Id).ToArray());
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].Embedding!.ToArray());
        Assert.Equal(new Half[] { (Half)1, (Half)1, (Half)1 }, items[0].HalfEmbedding!.ToArray());
        Assert.Equal(new BitArray(new bool[] { false, false, false }), items[0].BinaryEmbedding!);
        Assert.Equal(new float[] { 1, 1, 1 }, items[0].SparseEmbedding!.ToArray());

        conn.Execute("CREATE INDEX ON dapper_items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100)");
    }
}
