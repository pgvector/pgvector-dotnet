# pgvector-dotnet

[pgvector](https://github.com/pgvector/pgvector) support for C#

Supports [Npgsql](https://github.com/npgsql/npgsql)

[![Build Status](https://github.com/pgvector/pgvector-dotnet/workflows/build/badge.svg?branch=master)](https://github.com/pgvector/pgvector-dotnet/actions)

## Getting Started

Run:

```sh
dotnet add package Pgvector
```

And follow the instructions for your database library:

- [Npgsql](#npgsql)

## Npgsql

Import the library

```csharp
using Pgvector.Npgsql;
```

Create a table

```csharp
await using (var cmd = new NpgsqlCommand("CREATE TABLE items (embedding vector(3))", conn))
{
    await cmd.ExecuteNonQueryAsync();
}
```

Insert a vector

```csharp
await using (var cmd = new NpgsqlCommand("INSERT INTO items (embedding) VALUES ($1)", conn))
{
    var embedding = new Vector(new float[] { 1, 1, 1 });
    cmd.Parameters.AddWithValue(embedding);
    await cmd.ExecuteNonQueryAsync();
}
```

Get the nearest neighbors

```csharp
await using (var cmd = new NpgsqlCommand("SELECT * FROM items ORDER BY embedding <-> $1 LIMIT 5", conn))
{
    var embedding = new Vector(new float[] { 1, 1, 1 });
    cmd.Parameters.AddWithValue(embedding);

    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
            Console.WriteLine((Vector) reader.GetValue(0));
    }
}
```

Add an approximate index

```csharp
await using (var cmd = new NpgsqlCommand("CREATE INDEX my_index ON items USING ivfflat (embedding vector_l2_ops)", conn))
{
    await cmd.ExecuteNonQueryAsync();
}
```

Use `vector_ip_ops` for inner product and `vector_cosine_ops` for cosine distance

See a [full example](https://github.com/pgvector/pgvector-dotnet/blob/master/tests/Pgvector.Tests/PgvectorTests.cs)

## History

View the [changelog](https://github.com/pgvector/pgvector-dotnet/blob/master/CHANGELOG.md)

## Contributing

Everyone is encouraged to help improve this project. Here are a few ways you can help:

- [Report bugs](https://github.com/pgvector/pgvector-dotnet/issues)
- Fix bugs and [submit pull requests](https://github.com/pgvector/pgvector-dotnet/pulls)
- Write, clarify, or fix documentation
- Suggest or add new features

To get started with development:

```sh
git clone https://github.com/pgvector/pgvector-dotnet.git
cd pgvector-dotnet
createdb pgvector_dotnet_test
dotnet test
```
