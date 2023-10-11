using System;
using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Pgvector.Npgsql;

public class VectorTypeInfoResolver : IPgTypeInfoResolver
{
    TypeInfoMappingCollection Mappings { get; }

    public VectorTypeInfoResolver()
    {
        Mappings = new TypeInfoMappingCollection();
        AddInfos(Mappings);
        // TODO: Opt-in only
        AddArrayInfos(Mappings);
    }

    public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
        => Mappings.Find(type, dataTypeName, options);

    static void AddInfos(TypeInfoMappingCollection mappings)
        => mappings.AddType<Vector>("vector",
            static (options, mapping, _) => mapping.CreateInfo(options, new VectorConverter()), isDefault: true);

    static void AddArrayInfos(TypeInfoMappingCollection mappings)
        => mappings.AddArrayType<Vector>("vector");
}
