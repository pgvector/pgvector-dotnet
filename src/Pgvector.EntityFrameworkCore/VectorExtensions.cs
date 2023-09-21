namespace Pgvector.EntityFrameworkCore;

public static class VectorExtensions
{
    /// <summary>
    /// cosine_distance (&lt;=&gt;)
    /// </summary>
    public static double CosineDistance(this Vector a, Vector b)
    {
        throw new InvalidOperationException("This method can only be used in Linq expressions");
    }

    /// <summary>
    /// l2_distance (&lt;-&gt;)
    /// </summary>
    public static double L2Distance(this Vector a, Vector b)
    {
        throw new InvalidOperationException("This method can only be used in Linq expressions");
    }

    /// <summary>
    /// vector_negative_inner_product (&lt;#&gt;)
    /// </summary>
    public static double MaxInnerProduct(this Vector a, Vector b)
    {
        throw new InvalidOperationException("This method can only be used in Linq expressions");
    }
}

