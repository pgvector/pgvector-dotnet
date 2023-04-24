using Npgsql;
using Npgsql.BackendMessages;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;

namespace Pgvector.Npgsql;

public partial class VectorHandler : NpgsqlSimpleTypeHandler<Vector>
{
    public VectorHandler(PostgresType pgType) : base(pgType) { }

    public override Vector Read(NpgsqlReadBuffer buf, int len, FieldDescription? fieldDescription = null)
    {
        var dim = buf.ReadUInt16();
        var unused = buf.ReadUInt16();
        if (unused != 0)
            throw new InvalidCastException("expected unused to be 0");

        var vec = new float[dim];
        for (int i = 0; i < dim; i++)
            vec[i] = buf.ReadSingle();

        return new Vector(vec);
    }

    public override int ValidateAndGetLength(Vector value, NpgsqlParameter? parameter)
        => sizeof(UInt16) * 2 + sizeof(Single) * value.ToArray().Length;

    public override void Write(Vector value, NpgsqlWriteBuffer buf, NpgsqlParameter? parameter)
    {
        var vec = value.ToArray();
        var dim = vec.Length;
        buf.WriteUInt16(Convert.ToUInt16(dim));
        buf.WriteUInt16(0);

        for (int i = 0; i < dim; i++)
            buf.WriteSingle(vec[i]);
    }

    public override int ValidateObjectAndGetLength(object? value, ref NpgsqlLengthCache? lengthCache, NpgsqlParameter? parameter)
        => value switch
        {
            Vector converted => ValidateAndGetLength(converted, parameter),
            DBNull => 0,
            null => 0,
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type VectorHandler")
        };

    public override Task WriteObjectWithLength(object? value, NpgsqlWriteBuffer buf, NpgsqlLengthCache? lengthCache, NpgsqlParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => value switch
        {
            Vector converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
            DBNull => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            null => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type VectorHandler")
        };
}
