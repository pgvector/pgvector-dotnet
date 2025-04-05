using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Pgvector.EntityFrameworkCore;

public class VectorDataSourceConfigurationPlugin : INpgsqlDataSourceConfigurationPlugin
{
    public void Configure(NpgsqlDataSourceBuilder npgsqlDataSourceBuilder)
        => npgsqlDataSourceBuilder.UseVector();
}
