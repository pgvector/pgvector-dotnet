// good resources
// https://opensearch.org/blog/improving-document-retrieval-with-sparse-semantic-encoders/
// https://huggingface.co/opensearch-project/opensearch-neural-sparse-encoding-v1
//
// run with
// text-embeddings-router --model-id opensearch-project/opensearch-neural-sparse-encoding-v1 --pooling splade

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class ApiElement
{
    public required int index { get; set; }
    public required float value { get; set; }
}

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

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS documents", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE documents (id bigserial PRIMARY KEY, content text, embedding sparsevec(30522))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        string[] input = {
            "The dog is barking",
            "The cat is purring",
            "The bear is growling"
        };
        var embeddings = await FetchEmbeddings(input);

        for (int i = 0; i < input.Length; i++)
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(input[i]);
                cmd.Parameters.AddWithValue(new SparseVector(embeddings[i], 30522));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        var query = "forest";
        var queryEmbeddings = await FetchEmbeddings(new string[] { query });
        await using (var cmd = new NpgsqlCommand("SELECT content FROM documents ORDER BY embedding <#> $1 LIMIT 5", conn))
        {
            cmd.Parameters.AddWithValue(new SparseVector(queryEmbeddings[0], 30522));

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine((string)reader.GetValue(0));
                }
            }
        }
    }

    private static async Task<Dictionary<int, float>[]> FetchEmbeddings(string[] inputs)
    {
        var url = "http://localhost:3000/embed_sparse";
        var data = new
        {
            inputs = inputs
        };
        var client = new HttpClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiElement[][]>();
        return apiResponse!.Select(v => v.ToDictionary(e => e.index, e => e.value)).ToArray();
    }
}
