using Xunit;
using Dapper;
using Npgsql;
using Pgvector.Dapper;
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
        SqlMapper.AddTypeHandler(new VectorTypeHandler());

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
        conn.Execute(@"INSERT INTO dapper_items (embedding) VALUES (@embedding1), (@embedding2), (@embedding3)", new { embedding1, embedding2, embedding3 });

        var embedding = new Vector(new float[] { 1, 1, 1 });
        var items = conn.Query<DapperItem>("SELECT * FROM dapper_items ORDER BY embedding <-> @embedding LIMIT 5", new { embedding });
        foreach (DapperItem item in items)
        {
            Console.WriteLine(item.Embedding);
        }

        conn.Execute("CREATE INDEX ON dapper_items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100)");
    }
}
