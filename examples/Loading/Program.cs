class Program
{
    static async Task Main()
    {
        // generate random data
        var rows = 1000000;
        var dimensions = 128;
        var rand = new Random();
        var embeddings = Enumerable.Range(0, rows).Select((r) => Enumerable.Range(0, dimensions).Select((d) => (float)rand.NextDouble()).ToArray());

        // connect
        var connString = "Host=localhost;Database=pgvector_example";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();
        var conn = dataSource.OpenConnection();

        // enable extension
        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        conn.ReloadTypes();

        // create table
        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS items", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = new NpgsqlCommand($"CREATE TABLE items (id bigserial, embedding vector({dimensions}))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // load data
        Console.WriteLine($"Loading {rows} rows");
        await using (var writer = conn.BeginBinaryImport("COPY items (embedding) FROM STDIN WITH (FORMAT BINARY)"))
        {
            var i = 0;
            foreach (var embedding in embeddings)
            {
                // show progress
                if (i++ % 10000 == 0)
                    Console.Write(".");

                writer.StartRow();
                writer.Write(new Vector(embedding));
            }

            writer.Complete();
        }
        Console.WriteLine("\nSuccess!");

        // create any indexes *after* loading initial data
        if (Environment.GetEnvironmentVariable("TEST_LOADING") == "index")
        {
            Console.WriteLine("Creating index");
            await using (var cmd = new NpgsqlCommand("SET maintenance_work_mem = '8GB'", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            await using (var cmd = new NpgsqlCommand("SET max_parallel_maintenance_workers = 7", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            await using (var cmd = new NpgsqlCommand("CREATE INDEX ON items USING hnsw (embedding vector_cosine_ops)", conn))
            {
                cmd.CommandTimeout = 300;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // update planner statistics for good measure
        await using (var cmd = new NpgsqlCommand("ANALYZE items", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
