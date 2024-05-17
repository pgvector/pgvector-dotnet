using Npgsql.TypeMapping;
using Pgvector.Npgsql;

namespace Npgsql;

public static class VectorExtensions
{
    public static INpgsqlTypeMapper UseVector(this INpgsqlTypeMapper mapper)
    {
        mapper.AddTypeInfoResolverFactory(new VectorTypeInfoResolverFactory());
        mapper.AddTypeInfoResolverFactory(new HalfvecTypeInfoResolverFactory());
        mapper.AddTypeInfoResolverFactory(new SparsevecTypeInfoResolverFactory());
        return mapper;
    }
}
