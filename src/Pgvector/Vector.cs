using System;
using System.Globalization;
using System.Linq;

namespace Pgvector;

public class Vector
{
    private float[] vec;

    public Vector(float[] v)
        => vec = v;

    public Vector(string s)
        => vec = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v, CultureInfo.InvariantCulture));

    public override string ToString()
        => string.Concat("[", string.Join(",", vec.Select(v => v.ToString(CultureInfo.InvariantCulture))), "]");

    public float[] ToArray()
        => vec;

    public override bool Equals(object? obj)
    {
        if (obj is not Vector vector)
        {
            return false;
        }

        var arrayA = vec;
        var arrayB = vector.ToArray();

        if (arrayA.Length != arrayB.Length)
        {
            return false;
        }

        return Enumerable.SequenceEqual(arrayA, arrayB);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var value in vec)
        {
            hashCode.Add(value.GetHashCode());
        }

        return hashCode.ToHashCode();
    }
}
