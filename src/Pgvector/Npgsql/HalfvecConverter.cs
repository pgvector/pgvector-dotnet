using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql.Internal;

namespace Pgvector.Npgsql;

public class HalfvecConverter : PgStreamingConverter<HalfVector>
{
    public override HalfVector Read(PgReader reader)
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
            reader.Buffer(2 * sizeof(ushort));

        var dim = reader.ReadUInt16();
        var unused = reader.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new Half[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(ushort)))
                reader.Buffer(sizeof(ushort));
            vec[i] = BitConverter.UInt16BitsToHalf(reader.ReadUInt16());
        }

        return new HalfVector(vec);
    }

    public override async ValueTask<HalfVector> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
            await reader.BufferAsync(2 * sizeof(ushort), cancellationToken).ConfigureAwait(false);

        var dim = reader.ReadUInt16();
        var unused = reader.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new Half[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(ushort)))
                await reader.BufferAsync(sizeof(ushort), cancellationToken).ConfigureAwait(false);
            vec[i] = BitConverter.UInt16BitsToHalf(reader.ReadUInt16());
        }

        return new HalfVector(vec);
    }

    public override Size GetSize(SizeContext context, HalfVector value, ref object? writeState)
        => sizeof(ushort) * 2 + sizeof(ushort) * value.ToArray().Length;

    public override void Write(PgWriter writer, HalfVector value)
    {
        if (writer.ShouldFlush(sizeof(ushort) * 2))
            writer.Flush();

        var span = value.Memory.Span;
        var dim = span.Length;
        writer.WriteUInt16(Convert.ToUInt16(dim));
        writer.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
        {
            if (writer.ShouldFlush(sizeof(ushort)))
                writer.Flush();
            writer.WriteUInt16(BitConverter.HalfToUInt16Bits(span[i]));
        }
    }

    public override async ValueTask WriteAsync(
        PgWriter writer, HalfVector value, CancellationToken cancellationToken = default)
    {
        if (writer.ShouldFlush(sizeof(ushort) * 2))
            await writer.FlushAsync(cancellationToken);

        var memory = value.Memory;
        var dim = memory.Length;
        writer.WriteUInt16(Convert.ToUInt16(dim));
        writer.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
        {
            if (writer.ShouldFlush(sizeof(ushort)))
                await writer.FlushAsync(cancellationToken);
            writer.WriteUInt16(BitConverter.HalfToUInt16Bits(memory.Span[i]));
        }
    }
}
