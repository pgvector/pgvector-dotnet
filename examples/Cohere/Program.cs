using System.Collections;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class EmbedResponse
{
    public required EmbeddingsObject embeddings { get; set; }
}

class EmbeddingsObject
{
    public required int[][] ubinary { get; set; }
}

class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("CO_API_KEY");
        if (apiKey is null)
            throw new Exception("Set CO_API_KEY");

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

        await using (var cmd = new NpgsqlCommand("CREATE TABLE documents (id bigserial PRIMARY KEY, content text, embedding bit(1024))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        string[] input = {
            "The dog is barking",
            "The cat is purring",
            "The bear is growling"
        };
        var embeddings = await FetchEmbeddings(input, "search_document", apiKey);

        for (int i = 0; i < input.Length; i++)
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(input[i]);
                cmd.Parameters.AddWithValue(new BitArray(embeddings[i]));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        var query = "forest";
        var queryEmbedding = await FetchEmbeddings(new string[] { query }, "search_query", apiKey);

        await using (var cmd = new NpgsqlCommand("SELECT content FROM documents ORDER BY embedding <~> $1 LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(new BitArray(queryEmbedding[0]));

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine((string)reader.GetValue(0));
                }
            }
        }
    }

    private static async Task<byte[][]> FetchEmbeddings(string[] texts, string inputType, string apiKey)
    {
        var url = "https://api.cohere.com/v1/embed";
        var data = new
        {
            texts = texts,
            model = "embed-english-v3.0",
            input_type = inputType,
            embedding_types = new string[] { "ubinary" }
        };
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        using HttpResponseMessage response = await client.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<EmbedResponse>();
        return apiResponse!.embeddings.ubinary.Select(e => e.Select(v => (byte)v).ToArray()).ToArray();
    }
}
