using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Pgvector.EntityFrameworkCore;

public class SparsevecTypeMapping : RelationalTypeMapping
{
    public static SparsevecTypeMapping Default { get; } = new();

    public SparsevecTypeMapping() : base("sparsevec", typeof(SparseVector)) { }

    public SparsevecTypeMapping(string storeType) : base(storeType, typeof(SparseVector)) { }

    protected SparsevecTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SparsevecTypeMapping(parameters);
}
