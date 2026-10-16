using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Pgvector.EntityFrameworkCore;

public class VectorConventionSetPlugin : IConventionSetPlugin
{
    private readonly ProviderConventionSetBuilderDependencies _dependencies;

    public VectorConventionSetPlugin(ProviderConventionSetBuilderDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public virtual ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.Add(new VectorAutoLoadConvention(_dependencies));
        return conventionSet;
    }

    private class VectorAutoLoadConvention : AutoLoadConvention
    {
        public VectorAutoLoadConvention(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies) { }

        protected override bool ShouldBeAutoLoaded(IConventionProperty property)
            => property.ClrType != typeof(Vector)
                && property.ClrType != typeof(HalfVector)
                && property.ClrType != typeof(SparseVector);
    }
}
