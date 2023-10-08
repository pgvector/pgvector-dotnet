using Npgsql.TypeMapping;
using Pgvector.Npgsql;

namespace Npgsql;

public static class VectorExtensions
{
    public static INpgsqlTypeMapper UseVector(this INpgsqlTypeMapper mapper)
    {
        mapper.AddTypeResolverFactory(new VectorTypeHandlerResolverFactory());
        return mapper;
    }
}
