class Program
{
    static async Task Main()
    {
        // generate random data
        var rows = 1000000;
        var dimensions = 128;
        var rand = new Random();
        var embeddings = Enumerable.Range(0, rows).Select((r) => Enumerable.Range(0, dimensions).Select((d) => (float)rand.NextDouble()).ToArray());
        var categories = Enumerable.Range(0, rows).Select((r) => (long)rand.Next(100)).ToArray();
        var queries = Enumerable.Range(0, 10).Select((r) => Enumerable.Range(0, dimensions).Select((d) => (float)rand.NextDouble()).ToArray());

        // enable extensions
        var connString = "Host=localhost;Database=pgvector_citus";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();
        var conn = dataSource.OpenConnection();
        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS citus", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // GUC variables set on the session do not propagate to Citus workers
        // https://github.com/citusdata/citus/issues/462
        // you can either:
        // 1. set them on the system, user, or database and reconnect
        // 2. set them for a transaction with SET LOCAL
        await using (var cmd = new NpgsqlCommand("ALTER DATABASE pgvector_citus SET maintenance_work_mem = '512MB'", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("ALTER DATABASE pgvector_citus SET hnsw.ef_search = 20", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        conn.Close();

        // reconnect for updated GUC variables to take effect
        conn = dataSource.OpenConnection();

        Console.WriteLine("Creating distributed table");
        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS items", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand($"CREATE TABLE items (id bigserial, embedding vector({dimensions}), category_id bigint, PRIMARY KEY (id, category_id))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("SET citus.shard_count = 4", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand("SELECT create_distributed_table('items', 'category_id')", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("Loading data in parallel");
        await using (var writer = conn.BeginBinaryImport("COPY items (embedding, category_id) FROM STDIN WITH (FORMAT BINARY)"))
        {
            foreach (var (embedding, category) in embeddings.Zip(categories))
            {
                writer.StartRow();
                writer.Write(new Vector(embedding));
                writer.Write(category);
            }
            writer.Complete();
        }

        Console.WriteLine("Creating index in parallel");
        await using (var cmd = new NpgsqlCommand("CREATE INDEX ON items USING hnsw (embedding vector_l2_ops)", conn))
        {
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("Running distributed queries");
        foreach (var query in queries)
        {
            await using (var cmd = new NpgsqlCommand("SELECT id FROM items ORDER BY embedding <-> $1 LIMIT 5", conn))
            {
                cmd.Parameters.AddWithValue(new Vector(query));

                var ids = new List<long>();
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ids.Add((long)reader.GetValue(0));
                    }
                }
                Console.WriteLine(String.Join(", ", ids));
            }
        }
    }
}
