using System;
using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Pgvector.Npgsql;

public class VectorTypeInfoResolverFactory : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateResolver() => new Resolver();
    public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

    class Resolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddType<Vector>("vector",
                static (options, mapping, _) => mapping.CreateInfo(options, new VectorConverter()), isDefault: true);
#if NET5_0_OR_GREATER
            mappings.AddType<HalfVector>("halfvec",
                static (options, mapping, _) => mapping.CreateInfo(options, new HalfvecConverter()), isDefault: true);
#endif
            mappings.AddType<SparseVector>("sparsevec",
                static (options, mapping, _) => mapping.CreateInfo(options, new SparsevecConverter()), isDefault: true);
            return mappings;
        }
    }

    sealed class ArrayResolver : Resolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddArrayType<Vector>("vector");
#if NET5_0_OR_GREATER
            mappings.AddArrayType<HalfVector>("halfvec");
#endif
            mappings.AddArrayType<SparseVector>("sparsevec");
            return mappings;
        }
    }
}
