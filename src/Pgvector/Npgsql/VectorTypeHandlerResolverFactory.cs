using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.Internal.TypeMapping;
using System;

namespace Pgvector.Npgsql;

public class VectorTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    public override TypeHandlerResolver Create(TypeMapper typeMapper, NpgsqlConnector connector)
        => new VectorTypeHandlerResolver(connector);

    public override TypeMappingResolver CreateMappingResolver()
        => new VectorTypeMappingResolver();

    public override TypeMappingResolver CreateGlobalMappingResolver()
        => new VectorTypeMappingResolver();
}
