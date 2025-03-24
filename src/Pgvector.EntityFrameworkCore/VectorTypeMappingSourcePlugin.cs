using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (mappingInfo.StoreTypeName is not null)
        {
            VectorTypeMapping? mapping = (mappingInfo.StoreTypeNameBase ?? mappingInfo.StoreTypeName) switch
            {
                "vector" => new(mappingInfo.StoreTypeName, typeof(Vector), mappingInfo.Size),
                "halfvec" => new(mappingInfo.StoreTypeName, typeof(HalfVector), mappingInfo.Size),
                "sparsevec" => new(mappingInfo.StoreTypeName, typeof(SparseVector), mappingInfo.Size),
                _ => null,
            };

            // If the caller hasn't specified a CLR type (this is scaffolding), or if the user has specified
            // the one matching the store type, return the mapping.
            return mappingInfo.ClrType is null || mappingInfo.ClrType == mapping?.ClrType
                ? mapping : null;
        }

        // No store type specified, look up by the CLR type only
        return mappingInfo.ClrType switch
        {
            var t when t == typeof(Vector) => new VectorTypeMapping("vector", typeof(Vector), mappingInfo.Size),
            var t when t == typeof(HalfVector) => new VectorTypeMapping("halfvec", typeof(HalfVector), mappingInfo.Size),
            var t when t == typeof(SparseVector) => new VectorTypeMapping("sparsevec", typeof(SparseVector), mappingInfo.Size),
            _ => null,
        };
    }
}
