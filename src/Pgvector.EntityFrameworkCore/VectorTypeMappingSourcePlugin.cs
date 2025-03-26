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
                "vector" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "vector", typeof(Vector)),
                "halfvec" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "halfvec", typeof(HalfVector)),
                "sparsevec" => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec", typeof(SparseVector)),
                _ => null,
            };
        }

        return mappingInfo.ClrType switch
        {
            var t when t == typeof(Vector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "vector", typeof(Vector)),
            var t when t == typeof(HalfVector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "halfvec", typeof(HalfVector)),
            var t when t == typeof(SparseVector) => new VectorTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec", typeof(SparseVector)),
            _ => null,
        };
    }
}
