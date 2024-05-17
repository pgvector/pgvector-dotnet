using Microsoft.EntityFrameworkCore.Storage;

namespace Pgvector.EntityFrameworkCore;

public class SparsevecTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.ClrType == typeof(SparseVector)
            ? new SparsevecTypeMapping(mappingInfo.StoreTypeName ?? "sparsevec")
            : null;
}
