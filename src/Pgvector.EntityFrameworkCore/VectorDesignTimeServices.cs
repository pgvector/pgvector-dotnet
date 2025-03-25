using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Pgvector.EntityFrameworkCore;

public class VectorDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<IRelationalTypeMappingSourcePlugin, VectorTypeMappingSourcePlugin>()
            .AddSingleton<IProviderCodeGeneratorPlugin, VectorCodeGeneratorPlugin>();
}
