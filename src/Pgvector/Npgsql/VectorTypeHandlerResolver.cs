using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;
using System;

namespace Pgvector.Npgsql;

public class VectorTypeHandlerResolver : TypeHandlerResolver
{
    readonly NpgsqlDatabaseInfo _databaseInfo;
    readonly VectorHandler? _vectorHandler;

    internal VectorTypeHandlerResolver(NpgsqlConnector connector)
    {
        _databaseInfo = connector.DatabaseInfo;

        var pgVectorType = PgType("vector");
        if (pgVectorType != null)
        {
            _vectorHandler = new VectorHandler(pgVectorType);
        }
    }

    public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName)
        => typeName == "vector" ? _vectorHandler : null;

    public override NpgsqlTypeHandler? ResolveByClrType(Type type)
    {
        var dataTypeName = VectorTypeMappingResolver.ClrTypeToDataTypeName(type);
        if (dataTypeName != null)
        {
            var handler = ResolveByDataTypeName(dataTypeName);
            if (handler != null)
                return handler;
        }

        return null;
    }

   PostgresType? PgType(string pgTypeName)
        => _databaseInfo.TryGetPostgresTypeByName(pgTypeName, out var pgType) ? pgType : null;
}
