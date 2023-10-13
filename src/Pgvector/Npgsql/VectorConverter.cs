using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql.Internal;

namespace Pgvector.Npgsql;

public class VectorConverter : PgStreamingConverter<Vector>
{
    public override Vector Read(PgReader reader)
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
            reader.Buffer(2 * sizeof(ushort));

        var dim = reader.ReadUInt16();
        var unused = reader.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new float[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
                reader.Buffer(sizeof(float));
            vec[i] = reader.ReadFloat();
        }

        return new Vector(vec);
    }

    public override async ValueTask<Vector> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
            await reader.BufferAsync(2 * sizeof(ushort), cancellationToken).ConfigureAwait(false);

        var dim = reader.ReadUInt16();
        var unused = reader.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new float[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
                await reader.BufferAsync(sizeof(float), cancellationToken).ConfigureAwait(false);
            vec[i] = reader.ReadFloat();
        }

        return new Vector(vec);
    }

    public override Size GetSize(SizeContext context, Vector value, ref object? writeState)
        => sizeof(ushort) * 2 + sizeof(float) * value.ToArray().Length;

    public override void Write(PgWriter writer, Vector value)
    {
        if (writer.ShouldFlush(sizeof(ushort) * 2))
            writer.Flush();

        var span = value.Memory.Span;
        var dim = span.Length;
        writer.WriteUInt16(Convert.ToUInt16(dim));
        writer.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
        {
            if (writer.ShouldFlush(sizeof(float)))
                writer.Flush();
            writer.WriteFloat(span[i]);
        }
    }

    public override async ValueTask WriteAsync(
        PgWriter writer, Vector value, CancellationToken cancellationToken = default)
    {
        if (writer.ShouldFlush(sizeof(ushort) * 2))
            await writer.FlushAsync(cancellationToken);

        var memory = value.Memory;
        var dim = memory.Length;
        writer.WriteUInt16(Convert.ToUInt16(dim));
        writer.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
        {
            if (writer.ShouldFlush(sizeof(float)))
                await writer.FlushAsync(cancellationToken);
            writer.WriteFloat(memory.Span[i]);
        }
    }
}
