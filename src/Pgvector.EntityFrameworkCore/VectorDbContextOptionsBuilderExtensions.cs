using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Pgvector.Npgsql;

namespace Pgvector.EntityFrameworkCore;

public static class VectorDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder UseVector(this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        // not ideal, but how Npgsql.EntityFrameworkCore.PostgreSQL does it
        NpgsqlConnection.GlobalTypeMapper.UseVector();

        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<VectorDbContextOptionsExtension>()
            ?? new VectorDbContextOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
