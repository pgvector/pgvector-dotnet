using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class ApiResponse
{
    public required float[][] embeddings { get; set; }
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

        await using (var cmd = new NpgsqlCommand("CREATE TABLE documents (id bigserial PRIMARY KEY, content text, embedding vector(768))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE INDEX ON documents USING GIN (to_tsvector('english', content))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        string[] input = {
            "The dog is barking",
            "The cat is purring",
            "The bear is growling"
        };
        var embeddings = await Embed(input, "search_document");

        for (int i = 0; i < input.Length; i++)
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(input[i]);
                cmd.Parameters.AddWithValue(new Vector(embeddings[i]));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        var sql = @"
        WITH semantic_search AS (
            SELECT id, RANK () OVER (ORDER BY embedding <=> $2) AS rank
            FROM documents
            ORDER BY embedding <=> $2
            LIMIT 20
        ),
        keyword_search AS (
            SELECT id, RANK () OVER (ORDER BY ts_rank_cd(to_tsvector('english', content), query) DESC)
            FROM documents, plainto_tsquery('english', $1) query
            WHERE to_tsvector('english', content) @@ query
            ORDER BY ts_rank_cd(to_tsvector('english', content), query) DESC
            LIMIT 20
        )
        SELECT
            COALESCE(semantic_search.id, keyword_search.id) AS id,
            COALESCE(1.0 / ($3 + semantic_search.rank), 0.0) +
            COALESCE(1.0 / ($3 + keyword_search.rank), 0.0) AS score
        FROM semantic_search
        FULL OUTER JOIN keyword_search ON semantic_search.id = keyword_search.id
        ORDER BY score DESC
        LIMIT 5
        ";
        var query = "growling bear";
        var queryEmbedding = (await Embed(new string[] { query }, "search_query"))[0];
        var k = 60;
        await using (var cmd = new NpgsqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue(query);
            cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
            cmd.Parameters.AddWithValue(k);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine("document: {0}, RRF score: {1}", (long)reader.GetValue(0), (decimal)reader.GetValue(1));
                }
            }
        }
    }

    private static async Task<float[][]> Embed(string[] input, string taskType)
    {
        // nomic-embed-text uses a task prefix
        // https://huggingface.co/nomic-ai/nomic-embed-text-v1.5
        input = input.Select(v => taskType + ": " + v).ToArray();

        var url = "http://localhost:11434/api/embed";
        var data = new
        {
            input = input,
            model = "nomic-embed-text"
        };
        var client = new HttpClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return apiResponse!.embeddings.ToArray();
    }
}
