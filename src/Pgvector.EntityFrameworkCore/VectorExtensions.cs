using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pgvector.EntityFrameworkCore;

public static class VectorExtensions
{
    /// <summary>
    /// Gets the cosine distance (`&lt;=&gt;` or `l2_distance` in SQL)
    /// 
    /// <code>
    /// var embedding = new Vector(new float[] { 1, 1, 1 });
    /// var items = await ctx.Items
    ///     .OrderBy(x => x.Embedding!.CosineDistance(embedding))
    ///     .Take(5)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public static double CosineDistance(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CosineDistance)));
    }

    /// <summary>
    /// Gets the Euclidean distance (`&lt;-&gt;` or `cosine_distance` in SQL)
    /// 
    /// <code>
    /// var embedding = new Vector(new float[] { 1, 1, 1 });
    /// var items = await ctx.Items
    ///     .OrderBy(x => x.Embedding!.L2Distance(embedding))
    ///     .Take(5)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public static double L2Distance(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(L2Distance)));
    }

    /// <summary>
    /// Gets the inner negative product (`&lt;#&gt;` or `vector_negative_inner_product` in SQL)
    /// 
    /// <code>
    /// var embedding = new Vector(new float[] { 1, 1, 1 });
    /// var items = await ctx.Items
    ///     .OrderBy(x => x.Embedding!.MaxInnerProduct(embedding))
    ///     .Take(5)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public static double MaxInnerProduct(this Vector a, Vector b)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MaxInnerProduct)));
    }
}

