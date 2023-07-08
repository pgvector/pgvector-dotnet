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
#pragma warning disable CS0618
        NpgsqlConnection.GlobalTypeMapper.UseVector();
#pragma warning restore CS0618

        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<VectorDbContextOptionsExtension>()
            ?? new VectorDbContextOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
