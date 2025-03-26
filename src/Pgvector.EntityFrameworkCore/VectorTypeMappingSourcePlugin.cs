using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (mappingInfo.ClrType == null)
        {
            return (mappingInfo.StoreTypeNameBase ?? mappingInfo.StoreTypeName) switch
            {
                "vector" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "vector", typeof(Vector), mappingInfo.Size),
                "halfvec" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "halfvec", typeof(HalfVector), mappingInfo.Size),
                "sparsevec" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec", typeof(SparseVector), mappingInfo.Size),
                _ => null,
            };
        }

        return mappingInfo.ClrType switch
        {
            var t when t == typeof(Vector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "vector", typeof(Vector), mappingInfo.Size),
            var t when t == typeof(HalfVector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "halfvec", typeof(HalfVector), mappingInfo.Size),
            var t when t == typeof(SparseVector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec", typeof(SparseVector), mappingInfo.Size),
            _ => null,
        };
    }
}
