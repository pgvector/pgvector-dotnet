using Xunit;
using Dapper;
using Npgsql;
using Pgvector.Npgsql;

namespace Pgvector.Tests;

public class DapperItem
{
    public Vector? Embedding { get; set; }
}

public class DapperTests
{
    [Fact]
    public async Task Main()
    {
        var connString = "Host=localhost;Database=pgvector_dotnet_test";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        conn.Execute("CREATE EXTENSION IF NOT EXISTS vector");
        conn.ReloadTypes();

        conn.Execute("DROP TABLE IF EXISTS dapper_items");
        conn.Execute("CREATE TABLE dapper_items (embedding vector(3))");

        var embedding1 = new Vector(new float[] { 1, 1, 1 });
        var embedding2 = new Vector(new float[] { 2, 2, 2 });
        var embedding3 = new Vector(new float[] { 1, 1, 2 });
        conn.Execute(@"INSERT INTO dapper_items (embedding) VALUES (@e1::vector), (@e2::vector), (@e3::vector)", new { e1 = embedding1.ToString(), e2 = embedding2.ToString(), e3 = embedding3.ToString() });

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = conn.Query<DapperItem>("SELECT * FROM dapper_items ORDER BY embedding <-> @embedding::vector LIMIT 5", new { embedding = embedding.ToString() });
        foreach (DapperItem item in items)
            Console.WriteLine(item.Embedding);

        conn.Execute("CREATE INDEX ON dapper_items USING ivfflat (embedding vector_l2_ops)");
    }
}
