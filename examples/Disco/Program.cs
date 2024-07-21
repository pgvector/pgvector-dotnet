using DiscoRec;

class Program
{
    static async Task Main()
    {
        var connString = "Host=localhost;Database=pgvector_example";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        conn.ReloadTypes();

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS users", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS movies", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE users (id integer PRIMARY KEY, factors vector(20))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE movies (name text PRIMARY KEY, factors vector(20))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        var data = await Data.LoadMovieLens();
        var recommender = Recommender<int, string>.FitExplicit(data, new RecommenderOptions { Factors = 20 });

        foreach (var id in recommender.UserIds())
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO users (id, factors) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(id);
                cmd.Parameters.AddWithValue(new Vector(recommender.UserFactors(id)));

                await cmd.ExecuteNonQueryAsync();
            }
        }

        foreach (var id in recommender.ItemIds())
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO movies (name, factors) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(id);
                cmd.Parameters.AddWithValue(new Vector(recommender.ItemFactors(id)));

                await cmd.ExecuteNonQueryAsync();
            }
        }

        var movie = "Star Wars (1977)";
        Console.WriteLine("Item-based recommendations for {0}", movie);
        await using (var cmd = new NpgsqlCommand("SELECT name FROM movies WHERE name != $1 ORDER BY factors <=> (SELECT factors FROM movies WHERE name = $1) LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(movie);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine("- {0}", (string)reader.GetValue(0));
                }
            }
        }

        var userId = 123;
        Console.WriteLine("\nUser-based recommendations for user {0}", userId);
        await using (var cmd = new NpgsqlCommand("SELECT name FROM movies ORDER BY factors <#> (SELECT factors FROM users WHERE id = $1) LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(userId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine("- {0}", (string)reader.GetValue(0));
                }
            }
        }
    }
}
