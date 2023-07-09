using Pgvector.Npgsql;

namespace Pgvector.Tests;

public class NpgsqlTests
{
    [Fact]
    public async Task Main()
    {
        var connString = "Host=localhost;Database=pgvector_dotnet_test";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        conn.ReloadTypes();

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS items", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE items (embedding vector(3))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("INSERT INTO items (embedding) VALUES ($1), ($2), ($3)", conn))
        {
            var embedding1 = new Vector(new float[] { 1, 1, 1 });
            var embedding2 = new Vector(new float[] { 2, 2, 2 });
            var embedding3 = new Vector(new float[] { 1, 1, 2 });
            cmd.Parameters.AddWithValue(embedding1);
            cmd.Parameters.AddWithValue(embedding2);
            cmd.Parameters.AddWithValue(embedding3);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("SELECT * FROM items ORDER BY embedding <-> $1 LIMIT 5", conn))
        {
            var embedding = new Vector(new float[] { 1, 1, 1 });
            cmd.Parameters.AddWithValue(embedding);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var embeddings = new List<Vector>();

                while (await reader.ReadAsync())
                {
                    embeddings.Add((Vector)reader.GetValue(0));
                }

                Assert.Equal(new float[] { 1, 1, 1 }, embeddings[0].ToArray());
                Assert.Equal(new float[] { 1, 1, 2 }, embeddings[1].ToArray());
                Assert.Equal(new float[] { 2, 2, 2 }, embeddings[2].ToArray());
            }
        }

        await using (var cmd = new NpgsqlCommand("CREATE INDEX ON items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100)", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("SELECT $1", conn))
        {
            var embedding = new Vector(new float[16000]);
            cmd.Parameters.AddWithValue(embedding);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                Assert.Equal(embedding.ToArray(), ((Vector)reader.GetValue(0)).ToArray());
            }
        }

        await using (var cmd = new NpgsqlCommand("SELECT $1", conn))
        {
            var embedding = new Vector(new float[65536]);
            cmd.Parameters.AddWithValue(embedding);
            var exception = await Assert.ThrowsAsync<System.OverflowException>(() => cmd.ExecuteReaderAsync());
            Assert.Equal("Value was either too large or too small for a UInt16.", exception.Message);
        }
    }
}
