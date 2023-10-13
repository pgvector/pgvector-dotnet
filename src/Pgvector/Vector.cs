using System;
using System.Globalization;
using System.Linq;

namespace Pgvector;

public class Vector
{
    private ReadOnlyMemory<float> vec;

    public Vector(ReadOnlyMemory<float> v)
        => vec = v;

    public Vector(float[] v)
        => vec = new ReadOnlyMemory<float>(v);

    public Vector(string s)
        => new Vector(Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v, CultureInfo.InvariantCulture)));

    public override string ToString()
        => string.Concat("[", string.Join(",", vec.ToArray().Select(v => v.ToString(CultureInfo.InvariantCulture))), "]");

    public float[] ToArray()
        => vec.ToArray();
}
