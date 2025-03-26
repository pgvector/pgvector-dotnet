using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.ClrType == typeof(Vector) || (mappingInfo.ClrType == null && (mappingInfo.StoreTypeNameBase ?? mappingInfo.StoreTypeName) == "vector")
            ? new VectorTypeMapping(mappingInfo.StoreTypeName ?? "vector")
            : null;
}
