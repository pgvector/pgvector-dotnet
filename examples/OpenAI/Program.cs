using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class ApiResponse
{
    public required ApiObject[] data { get; set; }
}

class ApiObject
{
    public required float[] embedding { get; set; }
}

class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (apiKey is null)
            throw new Exception("Set OPENAI_API_KEY");

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

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS documents", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE documents (id bigserial PRIMARY KEY, content text, embedding vector(1536))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        string[] input = {
            "The dog is barking",
            "The cat is purring",
            "The bear is growling"
        };
        var embeddings = await FetchEmbeddings(input, apiKey);

        for (int i = 0; i < input.Length; i++)
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(input[i]);
                cmd.Parameters.AddWithValue(new Vector(embeddings[i]));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        var documentId = 2;
        await using (var cmd = new NpgsqlCommand("SELECT * FROM documents WHERE id != $1 ORDER BY embedding <=> (SELECT embedding FROM documents WHERE id = $1) LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(documentId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine((string)reader.GetValue(1));
                }
            }
        }
    }

    private static async Task<float[][]> FetchEmbeddings(string[] input, string apiKey)
    {
        var url = "https://api.openai.com/v1/embeddings";
        var data = new
        {
            input = input,
            model = "text-embedding-3-small"
        };
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        using HttpResponseMessage response = await client.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return apiResponse!.data.Select(e => e.embedding).ToArray();
    }
}
