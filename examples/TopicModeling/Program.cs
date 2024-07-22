// https://learn.microsoft.com/en-us/dotnet/api/microsoft.ml.textcatalog.latentdirichletallocation?view=ml-dotnet

using Microsoft.ML;

class LdaInput
{
    public string Text { get; set; }
}

class LdaOutput
{
    public float[] Features { get; set; }
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

        await using (var cmd = new NpgsqlCommand("CREATE TABLE documents (id bigserial PRIMARY KEY, content text, embedding vector(20))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        string[] input = {
            "The dog is barking",
            "The cat is purring",
            "The bear is growling"
        };
        var embeddings = GenerateEmbeddings(input);

        for (int i = 0; i < input.Length; i++)
        {
            await using (var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn))
            {
                cmd.Parameters.AddWithValue(input[i]);
                cmd.Parameters.AddWithValue(new Vector(embeddings[i]));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        var documentId = 1;
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

    private static float[][] GenerateEmbeddings(string[] texts)
    {
        var mlContext = new MLContext();
        var input = texts.Select((v) => new LdaInput { Text = v });
        var dataView = mlContext.Data.LoadFromEnumerable(input);
        var pipeline = mlContext.Transforms.Text.NormalizeText("NormalizedText", "Text")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
            .Append(mlContext.Transforms.Text.ProduceNgrams("Tokens"))
            .Append(mlContext.Transforms.Text.LatentDirichletAllocation("Features", "Tokens", numberOfTopics: 20));
        var model = pipeline.Fit(dataView);
        var engine = mlContext.Model.CreatePredictionEngine<LdaInput, LdaOutput>(model);
        return input.Select((v) => engine.Predict(v).Features).ToArray();
    }
}
