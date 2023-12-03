Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert
Imports Npgsql

Namespace Pgvector.VisualBasic.Tests
    <TestClass>
    Public Class NpgsqlTests
        <TestMethod>
        Sub TestSub()
            Dim connString = "Host=localhost;Database=pgvector_dotnet_test"

            Dim dataSourceBuilder As New NpgsqlDataSourceBuilder(connString)
            dataSourceBuilder.UseVector()
            Dim dataSource = dataSourceBuilder.Build()

            Dim conn = dataSource.OpenConnection()

            Using cmd As New NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn)
                cmd.ExecuteNonQuery()
            End Using

            conn.ReloadTypes()

            Using cmd As New NpgsqlCommand("DROP TABLE IF EXISTS vb_items", conn)
                cmd.ExecuteNonQuery()
            End Using

            Using cmd As New NpgsqlCommand("CREATE TABLE vb_items (id serial PRIMARY KEY, embedding vector(3))", conn)
                cmd.ExecuteNonQuery()
            End Using

            Using cmd As New NpgsqlCommand("INSERT INTO vb_items (embedding) VALUES ($1), ($2), ($3)", conn)
                Dim embedding1 As New Vector(New Single() {1, 1, 1})
                Dim embedding2 As New Vector(New Single() {2, 2, 2})
                Dim embedding3 As New Vector(New Single() {1, 1, 2})
                cmd.Parameters.AddWithValue(embedding1)
                cmd.Parameters.AddWithValue(embedding2)
                cmd.Parameters.AddWithValue(embedding3)
                cmd.ExecuteNonQuery()
            End Using

            Using cmd As New NpgsqlCommand("SELECT * FROM vb_items ORDER BY embedding <-> $1 LIMIT 5", conn)
                Dim embedding As New Vector(New Single() {1, 1, 1})
                cmd.Parameters.AddWithValue(embedding)

                Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                    Dim ids As New List(Of Integer)
                    Dim embeddings As New List(Of Vector)

                    While reader.Read()
                        ids.Add(reader.GetValue(0))
                        embeddings.Add(reader.GetValue(1))
                    End While

                    CollectionAssert.AreEqual(new Integer() {1, 3, 2}, ids.ToArray())
                    CollectionAssert.AreEqual(new Single() {1, 1, 1}, embeddings(0).ToArray())
                    CollectionAssert.AreEqual(new Single() {1, 1, 2}, embeddings(1).ToArray())
                    CollectionAssert.AreEqual(new Single() {2, 2, 2}, embeddings(2).ToArray())
                End Using
            End Using

            Using cmd As New NpgsqlCommand("CREATE INDEX ON vb_items USING hnsw (embedding vector_l2_ops)", conn)
                cmd.ExecuteNonQuery()
            End Using
        End Sub
    End Class
End Namespace
