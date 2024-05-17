using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Pgvector.EntityFrameworkCore;

public class VectorDbContextOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkRelationalServicesBuilder(services)
            .TryAdd<IMethodCallTranslatorPlugin, VectorDbFunctionsTranslatorPlugin>();

        services.AddSingleton<IRelationalTypeMappingSourcePlugin, VectorTypeMappingSourcePlugin>();
        services.AddSingleton<IRelationalTypeMappingSourcePlugin, HalfvecTypeMappingSourcePlugin>();
        services.AddSingleton<IRelationalTypeMappingSourcePlugin, SparsevecTypeMappingSourcePlugin>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        private new VectorDbContextOptionsExtension Extension
            => (VectorDbContextOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using vector ";

        public override int GetServiceProviderHashCode()
            => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Pgvector.EntityFrameworkCore:UseVector"] = "1";
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => true;
    }
}
