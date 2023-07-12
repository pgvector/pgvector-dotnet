using Npgsql;
using Npgsql.BackendMessages;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pgvector.Npgsql;

public class VectorHandler : NpgsqlTypeHandler<Vector>
{
    public VectorHandler(PostgresType pgType) : base(pgType) { }

    public override async ValueTask<Vector> Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
    {
        await buf.Ensure(2 * sizeof(ushort), async);
        var dim = buf.ReadUInt16();
        var unused = buf.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new float[dim];
        for (var i = 0; i < dim; i++)
        {
            await buf.Ensure(sizeof(float), async);
            vec[i] = buf.ReadSingle();
        }

        return new Vector(vec);
    }

    public override int ValidateAndGetLength(Vector value, ref NpgsqlLengthCache? lengthCache, NpgsqlParameter? parameter)
        => sizeof(ushort) * 2 + sizeof(float) * value.ToArray().Length;

    public override async Task Write(
        Vector value,
        NpgsqlWriteBuffer buf,
        NpgsqlLengthCache? lengthCache,
        NpgsqlParameter? parameter,
        bool async,
        CancellationToken cancellationToken = default)
    {
        if (buf.WriteSpaceLeft < sizeof(ushort) * 2)
            await buf.Flush(async, cancellationToken);

        var vec = value.ToArray();
        var dim = vec.Length;
        buf.WriteUInt16(Convert.ToUInt16(dim));
        buf.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
        {
            if (buf.WriteSpaceLeft < sizeof(float))
                await buf.Flush(async, cancellationToken);
            buf.WriteSingle(vec[i]);
        }
    }

    public override int ValidateObjectAndGetLength(object? value, ref NpgsqlLengthCache? lengthCache, NpgsqlParameter? parameter)
        => value switch
        {
            Vector converted => ValidateAndGetLength(converted, ref lengthCache, parameter),
            DBNull or null => 0,
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type VectorHandler")
        };

    public override Task WriteObjectWithLength(object? value, NpgsqlWriteBuffer buf, NpgsqlLengthCache? lengthCache, NpgsqlParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => value switch
        {
            Vector converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
            DBNull or null => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            _ => throw new InvalidCastException(
                $"Can't write CLR type {value.GetType()} with handler type VectorHandler")
        };
}
