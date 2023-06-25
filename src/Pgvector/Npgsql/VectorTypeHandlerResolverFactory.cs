using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using System;

namespace Pgvector.Npgsql;

public class VectorTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    public override TypeHandlerResolver Create(NpgsqlConnector connector)
        => new VectorTypeHandlerResolver(connector);

    public override string? GetDataTypeNameByClrType(Type type)
        => VectorTypeHandlerResolver.ClrTypeToDataTypeName(type);

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => VectorTypeHandlerResolver.DoGetMappingByDataTypeName(dataTypeName);
}
