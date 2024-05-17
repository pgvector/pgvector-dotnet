using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Pgvector.EntityFrameworkCore;

public class HalfvecTypeMapping : RelationalTypeMapping
{
    public static HalfvecTypeMapping Default { get; } = new();

    public HalfvecTypeMapping() : base("halfvec", typeof(HalfVector)) { }

    public HalfvecTypeMapping(string storeType) : base(storeType, typeof(HalfVector)) { }

    protected HalfvecTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new HalfvecTypeMapping(parameters);
}
