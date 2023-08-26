namespace Pgvector.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
public static class VectorExtensions
{
    /// <summary>
    /// cosine_distance (&lt;=&gt;)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static double CosineDistance(this Vector a, Vector b)
    {
        var arrayA = a.ToArray();
        var arrayB = b.ToArray();

        if (arrayA.Length != arrayB.Length)
        {
            throw new Exception("Inconsistent dimensions");
        }

        var dotProduct = 0.0;
        var normA = 0.0;
        var normB = 0.0;

        for (var i = 0; i < arrayA.Length; i++)
        {
            dotProduct += arrayA[i] * arrayB[i];
            normA += Math.Pow(arrayA[i], 2);
            normB += Math.Pow(arrayB[i], 2);
        }

        // TODO: think about it
        if (normA == 0.0 || normB == 0.0)
        {
            throw new ArgumentException("embedding must not have zero magnitude.");
        }

        var similarity = dotProduct / Math.Sqrt(normA * normB);

        similarity = Math.Clamp(similarity, -1.0, 1.0);

        var distance = 1.0 - similarity;
        return distance;
    }

    /// <summary>
    /// l2_distance (&lt;-&gt;)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static double EuclideanDistance(this Vector a, Vector b)
    {
        var distance = 0f;
        var arrayA = a.ToArray();
        var arrayB = b.ToArray();

        if (arrayA.Length != arrayB.Length)
        {
            throw new Exception("Inconsistent dimensions");
        }

        for (var i = 0; i < arrayA.Length; i++)
        {
            var diff = arrayA[i] - arrayB[i];
            distance += diff * diff;
        }

        return Math.Sqrt(distance);
    }

    /// <summary>
    /// vector_negative_inner_product (&lt;#&gt;)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double InnerProduct(this Vector a, Vector b)
    {
        var distance = 0f;
        var arrayA = a.ToArray();
        var arrayB = b.ToArray();

        if (arrayA.Length != arrayB.Length)
        {
            throw new Exception("Inconsistent dimensions");
        }

        for (var i = 0; i < arrayA.Length; i++)
        {
            distance += arrayA[i] * arrayB[i];
        }

        return distance * -1;
    }
}

