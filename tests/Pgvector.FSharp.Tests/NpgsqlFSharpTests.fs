module Pgvector.FSharp.Tests

open Npgsql
open Npgsql.FSharp
open NUnit.Framework
open Pgvector

type Item = {
    Id: int
    Embedding: Vector
}

[<Test>]
let Main () =
    let dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost;Database=pgvector_dotnet_test")
    dataSourceBuilder.UseVector() |> ignore
    use dataSource = dataSourceBuilder.Build()

    dataSource
    |> Sql.fromDataSource
    |> Sql.query "CREATE EXTENSION IF NOT EXISTS vector"
    |> Sql.executeNonQuery
    |> ignore

    dataSource
    |> Sql.fromDataSource
    |> Sql.query "DROP TABLE IF EXISTS fsharp_items"
    |> Sql.executeNonQuery
    |> ignore

    dataSource
    |> Sql.fromDataSource
    |> Sql.query "CREATE TABLE fsharp_items (id serial PRIMARY KEY, embedding vector(3))"
    |> Sql.executeNonQuery
    |> ignore

    let embedding1 = new Vector([| 1f; 1f; 1f |])
    let embedding2 = new Vector([| 2f; 2f; 2f |])
    let embedding3 = new Vector([| 1f; 1f; 2f |])

    let parameter1 = new NpgsqlParameter("", embedding1)
    let parameter2 = new NpgsqlParameter("", embedding2)
    let parameter3 = new NpgsqlParameter("", embedding3)

    dataSource
    |> Sql.fromDataSource
    |> Sql.query "INSERT INTO fsharp_items (embedding) VALUES (@embedding1), (@embedding2), (@embedding3)"
    |> Sql.parameters [ "embedding1", Sql.parameter parameter1; "embedding2", Sql.parameter parameter2;  "embedding3", Sql.parameter parameter3 ]
    |> Sql.executeNonQuery
    |> ignore

    let embedding = new Vector([| 1f; 1f; 1f |])
    let parameter = new NpgsqlParameter("", embedding)

    let items =
        dataSource
        |> Sql.fromDataSource
        |> Sql.query "SELECT * FROM fsharp_items ORDER BY embedding <-> @embedding LIMIT 5"
        |> Sql.parameters [ "embedding", Sql.parameter parameter ]
        |> Sql.execute (fun read ->
            {
                Id = read.int "id"
                Embedding = read.fieldValue<Vector> "embedding"
            })

    Assert.AreEqual([| 1; 3; 2 |], [| for i in items -> i.Id |])
    Assert.AreEqual([| 1f; 1f; 1f |], items[0].Embedding.ToArray())
    Assert.AreEqual([| 1f; 1f; 2f |], items[1].Embedding.ToArray())
    Assert.AreEqual([| 2f; 2f; 2f |], items[2].Embedding.ToArray())

    dataSource
    |> Sql.fromDataSource
    |> Sql.query "CREATE INDEX ON fsharp_items USING hnsw (embedding vector_l2_ops)"
    |> Sql.executeNonQuery
    |> ignore
