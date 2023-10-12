using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMapping : RelationalTypeMapping
{
    public VectorTypeMapping(string storeType) : base(storeType, typeof(Vector)) { }

    protected VectorTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new VectorTypeMapping(parameters);
}
