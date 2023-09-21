using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pgvector.EntityFrameworkCore;

public static class VectorExtensions
{
    /// <summary>
    /// cosine_distance (&lt;=&gt;)
    /// </summary>
    public static double CosineDistance(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CosineDistance)));
    }

    /// <summary>
    /// l2_distance (&lt;-&gt;)
    /// </summary>
    public static double L2Distance(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(L2Distance)));
    }

    /// <summary>
    /// vector_negative_inner_product (&lt;#&gt;)
    /// </summary>
    public static double MaxInnerProduct(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MaxInnerProduct)));
    }
}

