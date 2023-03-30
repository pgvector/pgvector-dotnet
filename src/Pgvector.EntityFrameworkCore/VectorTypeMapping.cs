using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMapping : NpgsqlTypeMapping
{
    public VectorTypeMapping(string storeType) : base(storeType, typeof(Vector), NpgsqlDbType.Unknown) { }

    protected VectorTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters, NpgsqlDbType.Unknown) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new VectorTypeMapping(parameters);
}
