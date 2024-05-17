using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pgvector.EntityFrameworkCore;

public static class VectorDbFunctionsExtensions
{
    public static double L2Distance(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(L2Distance)));

    public static double MaxInnerProduct(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MaxInnerProduct)));

    public static double CosineDistance(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CosineDistance)));

    public static double L1Distance(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(L1Distance)));

    public static double HammingDistance(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(HammingDistance)));

    public static double JaccardDistance(this object a, object b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JaccardDistance)));
}
