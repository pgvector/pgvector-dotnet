using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class HalfvecTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.ClrType == typeof(HalfVector) || (mappingInfo.ClrType == null && (mappingInfo.StoreTypeNameBase ?? mappingInfo.StoreTypeName) == "halfvec")
            ? new HalfvecTypeMapping(mappingInfo.StoreTypeName ?? "halfvec")
            : null;
}
