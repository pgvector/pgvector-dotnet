# pgvector-dotnet

[pgvector](https://github.com/pgvector/pgvector) support for C#

Supports [Npgsql](https://github.com/npgsql/npgsql), [Dapper](https://github.com/DapperLib/Dapper), and [Entity Framework Core](https://github.com/dotnet/efcore)

[![Build Status](https://github.com/pgvector/pgvector-dotnet/workflows/build/badge.svg?branch=master)](https://github.com/pgvector/pgvector-dotnet/actions)

## Getting Started

Follow the instructions for your database library:

- [Npgsql](#npgsql)
- [Dapper](#dapper)
- [Entity Framework Core](#entity-framework-core)

## Npgsql

Run:

```sh
dotnet add package Pgvector
```

Import the library

```csharp
using Pgvector.Npgsql;
```

Create a connection

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
dataSourceBuilder.UseVector();
await using var dataSource = dataSourceBuilder.Build();

var conn = dataSource.OpenConnection();
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
        {
            Console.WriteLine((Vector)reader.GetValue(0));
        }
    }
}
```

Add an approximate index

```csharp
await using (var cmd = new NpgsqlCommand("CREATE INDEX ON items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100)", conn))
{
    await cmd.ExecuteNonQueryAsync();
}
```

Use `vector_ip_ops` for inner product and `vector_cosine_ops` for cosine distance

See a [full example](https://github.com/pgvector/pgvector-dotnet/blob/master/tests/Pgvector.Tests/NpgsqlTests.cs)

## Dapper

Run:

```sh
dotnet add package Pgvector.Dapper
```

Import the library

```csharp
using Pgvector.Dapper;
using Pgvector.Npgsql;
```

Create a connection

```csharp
SqlMapper.AddTypeHandler(new VectorTypeHandler());

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
dataSourceBuilder.UseVector();
await using var dataSource = dataSourceBuilder.Build();

var conn = dataSource.OpenConnection();
```

Define a class

```csharp
public class Item
{
    public Vector? Embedding { get; set; }
}
```

Create a table

```csharp
conn.Execute("CREATE TABLE items (embedding vector(3))");
```

Insert a vector

```csharp
var embedding = new Vector(new float[] { 1, 1, 1 });
conn.Execute(@"INSERT INTO items (embedding) VALUES (@embedding)", new { embedding });
```

Get the nearest neighbors

```csharp
var embedding = new Vector(new float[] { 1, 1, 1 });
var items = conn.Query<Item>("SELECT * FROM items ORDER BY embedding <-> @embedding LIMIT 5", new { embedding });
foreach (Item item in items)
{
    Console.WriteLine(item.Embedding);
}
```

Add an approximate index

```csharp
conn.Execute("CREATE INDEX ON items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100)");
```

Use `vector_ip_ops` for inner product and `vector_cosine_ops` for cosine distance

See a [full example](https://github.com/pgvector/pgvector-dotnet/blob/master/tests/Pgvector.Tests/DapperTests.cs)

## Entity Framework Core

Run:

```sh
dotnet add package Pgvector.EntityFrameworkCore
```

Import the library

```csharp
using Pgvector.EntityFrameworkCore;
```

Configure the connection

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseNpgsql("connString", o => o.UseVector());
}
```

Define a model

```csharp
public class Item
{
    [Column(TypeName = "vector(3)")]
    public Vector? Embedding { get; set; }
}
```

Insert a vector

```csharp
ctx.Items.Add(new Item { Embedding = new Vector(new float[] { 1, 1, 1 }) });
ctx.SaveChanges();
```

Get the nearest neighbors

```csharp
var embedding = new Vector(new float[] { 1, 1, 1 });
var items = await ctx.Items.FromSql($"SELECT * FROM items ORDER BY embedding <-> {embedding} LIMIT 5").ToListAsync();
foreach (Item item in items)
{
    if (item.Embedding != null)
    {
        Console.WriteLine(item.Embedding);
    }
}
```

Add an approximate index

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Item>()
        .HasIndex(i => i.Embedding)
        .HasMethod("ivfflat")
        .HasOperators("vector_l2_ops");
}
```

Use `vector_ip_ops` for inner product and `vector_cosine_ops` for cosine distance

See a [full example](https://github.com/pgvector/pgvector-dotnet/blob/master/tests/Pgvector.Tests/EntityFrameworkCoreTests.cs)

## History

- [Pgvector](https://github.com/pgvector/pgvector-dotnet/blob/master/src/Pgvector/CHANGELOG.md)
- [Pgvector.Dapper](https://github.com/pgvector/pgvector-dotnet/blob/master/src/Pgvector.Dapper/CHANGELOG.md)
- [Pgvector.EntityFrameworkCore](https://github.com/pgvector/pgvector-dotnet/blob/master/src/Pgvector.EntityFrameworkCore/CHANGELOG.md)

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
