using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using System.Reflection;

namespace Pgvector.EntityFrameworkCore;

public class VectorDbFunctionsTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public VectorDbFunctionsTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource
    )
    {
        Translators = new[]
        {
            new VectorDbFunctionsTranslator(sqlExpressionFactory, typeMappingSource),
        };
    }

    public virtual IEnumerable<IMethodCallTranslator> Translators { get; }

    private class VectorDbFunctionsTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        private static readonly MethodInfo _methodL2Distance = typeof(VectorDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(VectorDbFunctionsExtensions.L2Distance), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })!;

        private static readonly MethodInfo _methodMaxInnerProduct = typeof(VectorDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(VectorDbFunctionsExtensions.MaxInnerProduct), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })!;

        private static readonly MethodInfo _methodCosineDistance = typeof(VectorDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(VectorDbFunctionsExtensions.CosineDistance), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })!;

        public VectorDbFunctionsTranslator(
            ISqlExpressionFactory sqlExpressionFactory,
            IRelationalTypeMappingSource typeMappingSource
        )
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

#pragma warning disable EF1001
        public SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger
        )
        {
            var vectorOperator = method switch
            {
                _ when ReferenceEquals(method, _methodL2Distance) => "<->",
                _ when ReferenceEquals(method, _methodMaxInnerProduct) => "<#>",
                _ when ReferenceEquals(method, _methodCosineDistance) => "<=>",
                _ => null
            };

            if (vectorOperator != null)
            {
                var resultTypeMapping = _typeMappingSource.FindMapping(method.ReturnType)!;

                return new PostgresUnknownBinaryExpression(
                    left: _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    right: _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    binaryOperator: vectorOperator,
                    type: resultTypeMapping.ClrType,
                    typeMapping: resultTypeMapping
                );
            }

            return null;
        }
#pragma warning restore EF1001
    }
}
