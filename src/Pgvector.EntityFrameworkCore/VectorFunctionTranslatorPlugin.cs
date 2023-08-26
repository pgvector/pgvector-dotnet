using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace Pgvector.EntityFrameworkCore;

public class VectorFunctionTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public VectorFunctionTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory, ITypeMappingSource typeMappingSource)
    {
        Translators = new[]
        {
            new VectorFunctionTranslator(sqlExpressionFactory, typeMappingSource),
        };
    }

    public virtual IEnumerable<IMethodCallTranslator> Translators { get; }

    private class VectorFunctionTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly ITypeMappingSource _typeMappingSource;

        private static readonly MethodInfo _methodEuclideanDistance = typeof(VectorExtensions)
            .GetRuntimeMethod(nameof(VectorExtensions.EuclideanDistance), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })
            ?? throw new InvalidOperationException($"Method {nameof(VectorExtensions.EuclideanDistance)} is not found");

        private static readonly MethodInfo _methodCosineDistance = typeof(VectorExtensions)
            .GetRuntimeMethod(nameof(VectorExtensions.CosineDistance), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })
            ?? throw new InvalidOperationException($"Method {nameof(VectorExtensions.CosineDistance)} is not found");

        private static readonly MethodInfo _methodInnerProduct = typeof(VectorExtensions)
            .GetRuntimeMethod(nameof(VectorExtensions.InnerProduct), new[]
            {
                typeof(Vector),
                typeof(Vector),
            })
            ?? throw new InvalidOperationException($"Method {nameof(VectorExtensions.InnerProduct)} is not found");

        public VectorFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory, ITypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger
        )
        {
            string? GetFunctionName()
            {
                if (ReferenceEquals(method, _methodEuclideanDistance))
                {
                    return "l2_distance";
                }

                if (ReferenceEquals(method, _methodCosineDistance))
                {
                    return "cosine_distance";
                }

                if (ReferenceEquals(method, _methodInnerProduct))
                {
                    return "vector_negative_inner_product";
                }

                return null;
            }

            var functionName = GetFunctionName();

            if (functionName != null)
            {
                var left = arguments[0];
                var right = arguments[1];

                var expression = _sqlExpressionFactory.Function(
                    name: functionName,
                    arguments: new[] { left, right },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false, false },
                    returnType: typeof(double),
                    typeMapping: left.TypeMapping
                );

                return expression;
            }

            return null;
        }
    }
}
