using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Reflection;

namespace Pgvector.EntityFrameworkCore;

public class VectorCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
{
    private static readonly MethodInfo _useVectorMethodInfo
        = typeof(VectorDbContextOptionsBuilderExtensions).GetMethod(
            nameof(VectorDbContextOptionsBuilderExtensions.UseVector),
            [typeof(NpgsqlDbContextOptionsBuilder)])!;

    public override MethodCallCodeFragment GenerateProviderOptions()
        => new(_useVectorMethodInfo);
}
