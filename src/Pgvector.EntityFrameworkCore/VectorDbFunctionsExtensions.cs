using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pgvector.EntityFrameworkCore;

public static class VectorDbFunctionsExtensions
{
    public static double L2Distance(this Vector a, Vector b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(L2Distance)));

    public static double MaxInnerProduct(this Vector a, Vector b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MaxInnerProduct)));

    public static double CosineDistance(this Vector a, Vector b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CosineDistance)));
}
