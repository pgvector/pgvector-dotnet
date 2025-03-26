using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Pgvector.EntityFrameworkCore;

public class VectorTypeMapping : RelationalTypeMapping
{
    public static VectorTypeMapping Default { get; } = new("vector", typeof(Vector));

    public VectorTypeMapping(string storeType, Type clrType, int? size = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType),
                storeType,
                StoreTypePostfix.Size,
                size: size,
                fixedLength: size is not null))
    {
    }

    protected VectorTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new VectorTypeMapping(parameters);
}
