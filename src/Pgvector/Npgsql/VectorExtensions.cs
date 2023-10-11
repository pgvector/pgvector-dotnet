using Npgsql.TypeMapping;

namespace Pgvector.Npgsql;

public static class VectorExtensions
{
    public static INpgsqlTypeMapper UseVector(this INpgsqlTypeMapper mapper)
    {
        mapper.AddTypeInfoResolver(new VectorTypeInfoResolver());
        return mapper;
    }
}
