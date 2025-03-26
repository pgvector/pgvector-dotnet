using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class SparsevecTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.ClrType == typeof(SparseVector) || (mappingInfo.ClrType == null && (mappingInfo.StoreTypeNameBase ?? mappingInfo.StoreTypeName) == "sparsevec")
            ? new SparsevecTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec")
            : null;
}
