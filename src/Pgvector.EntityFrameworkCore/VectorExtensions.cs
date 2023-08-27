namespace Pgvector.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
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
    public static double EuclideanDistance(this Vector a, Vector b)
    {
        throw new InvalidOperationException("This method can only be used in Linq expressions");
    }

    /// <summary>
    /// vector_negative_inner_product (&lt;#&gt;)
    /// </summary>
    public static double InnerProduct(this Vector a, Vector b)
    {
        throw new InvalidOperationException("This method can only be used in Linq expressions");
    }
}

