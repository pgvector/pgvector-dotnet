using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Pgvector.EntityFrameworkCore;
using Pgvector.Npgsql;

namespace Microsoft.EntityFrameworkCore;

public static class VectorDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder UseVector(this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<VectorDbContextOptionsExtension>()
            ?? new VectorDbContextOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
