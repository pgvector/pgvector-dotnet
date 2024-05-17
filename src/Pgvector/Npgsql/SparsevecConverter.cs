using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql.Internal;

namespace Pgvector.Npgsql;

public class SparsevecConverter : PgStreamingConverter<SparseVector>
{
    public override SparseVector Read(PgReader reader)
    {
        if (reader.ShouldBuffer(3 * sizeof(int)))
            reader.Buffer(3 * sizeof(int));

        var dim = reader.ReadInt32();
        var nnz = reader.ReadInt32();
        var unused = reader.ReadInt32();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var indices = new int[nnz];
        for (var i = 0; i < nnz; i++)
        {
            if (reader.ShouldBuffer(sizeof(int)))
                reader.Buffer(sizeof(int));
            indices[i] = reader.ReadInt32();
        }

        var values = new float[nnz];
        for (var i = 0; i < nnz; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
                reader.Buffer(sizeof(float));
            values[i] = reader.ReadFloat();
        }

        return new SparseVector(dim, indices, values);
    }

    public override async ValueTask<SparseVector> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
    {
        if (reader.ShouldBuffer(3 * sizeof(int)))
            await reader.BufferAsync(3 * sizeof(int), cancellationToken).ConfigureAwait(false);

        var dim = reader.ReadInt16();
        var nnz = reader.ReadInt16();
        var unused = reader.ReadInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var indices = new int[nnz];
        for (var i = 0; i < nnz; i++)
        {
            if (reader.ShouldBuffer(sizeof(int)))
                await reader.BufferAsync(sizeof(int), cancellationToken).ConfigureAwait(false); ;
            indices[i] = reader.ReadInt32();
        }

        var values = new float[nnz];
        for (var i = 0; i < nnz; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
                await reader.BufferAsync(sizeof(float), cancellationToken).ConfigureAwait(false);
            values[i] = reader.ReadFloat();
        }

        return new SparseVector(dim, indices, values);
    }

    public override Size GetSize(SizeContext context, SparseVector value, ref object? writeState)
        => sizeof(int) * 3 + sizeof(int) * value.Indices.Length + sizeof(float) * value.Values.Length;

    public override void Write(PgWriter writer, SparseVector value)
    {
        if (writer.ShouldFlush(sizeof(int) * 3))
            writer.Flush();

        var indicesSpan = value.Indices.Span;
        var valuesSpan = value.Values.Span;
        writer.WriteInt32(value.Dimensions);
        writer.WriteInt32(indicesSpan.Length);
        writer.WriteInt32(0);

        for (int i = 0; i < indicesSpan.Length; i++)
        {
            if (writer.ShouldFlush(sizeof(int)))
                writer.Flush();
            writer.WriteInt32(indicesSpan[i]);
        }

        for (int i = 0; i < valuesSpan.Length; i++)
        {
            if (writer.ShouldFlush(sizeof(float)))
                writer.Flush();
            writer.WriteFloat(valuesSpan[i]);
        }
    }

    public override async ValueTask WriteAsync(
        PgWriter writer, SparseVector value, CancellationToken cancellationToken = default)
    {
        if (writer.ShouldFlush(sizeof(int) * 3))
            await writer.FlushAsync(cancellationToken);

        var indices = value.Indices;
        var values = value.Values;
        writer.WriteInt32(value.Dimensions);
        writer.WriteInt32(indices.Length);
        writer.WriteInt32(0);

        for (int i = 0; i < indices.Length; i++)
        {
            if (writer.ShouldFlush(sizeof(int)))
                await writer.FlushAsync(cancellationToken);
            writer.WriteInt32(indices.Span[i]);
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (writer.ShouldFlush(sizeof(float)))
                await writer.FlushAsync(cancellationToken);
            writer.WriteFloat(values.Span[i]);
        }
    }
}
