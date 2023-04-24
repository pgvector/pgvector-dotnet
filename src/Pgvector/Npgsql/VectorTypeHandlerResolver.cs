using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;
using NpgsqlTypes;

namespace Pgvector.Npgsql;

public class VectorTypeHandlerResolver : TypeHandlerResolver
{
    readonly NpgsqlDatabaseInfo _databaseInfo;
    readonly VectorHandler? _vectorHandler;

    internal VectorTypeHandlerResolver(NpgsqlConnector connector)
    {
        _databaseInfo = connector.DatabaseInfo;

        var pgVectorType = PgType("vector");
        if (pgVectorType is not null)
        {
            _vectorHandler = new VectorHandler(pgVectorType);
        }
    }

    public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName)
        => typeName switch
        {
            "vector" => _vectorHandler,
            _ => null
        };

    public override NpgsqlTypeHandler? ResolveByClrType(Type type)
        => ClrTypeToDataTypeName(type) is { } dataTypeName && ResolveByDataTypeName(dataTypeName) is { } handler
            ? handler
            : null;

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => DoGetMappingByDataTypeName(dataTypeName);

    internal static string? ClrTypeToDataTypeName(Type type)
    {
        if (type == typeof(Vector))
        {
            return "vector";
        }

        return null;
    }

    internal static TypeMappingInfo? DoGetMappingByDataTypeName(string dataTypeName)
        => dataTypeName switch
        {
            "vector" => new(NpgsqlDbType.Unknown, "vector"),
            _ => null
        };

    PostgresType? PgType(string pgTypeName) => _databaseInfo.TryGetPostgresTypeByName(pgTypeName, out var pgType) ? pgType : null;
}
