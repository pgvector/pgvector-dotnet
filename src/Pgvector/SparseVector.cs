using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pgvector;

public class SparseVector
{
    public int Dimensions { get; }
    public ReadOnlyMemory<int> Indices { get; }
    public ReadOnlyMemory<float> Values { get; }

    public SparseVector(int dimensions, ReadOnlyMemory<int> indices, ReadOnlyMemory<float> values)
    {
        Dimensions = dimensions;
        Indices = indices;
        Values = values;
    }

    public SparseVector(ReadOnlyMemory<float> v)
    {
        var dense = v.Span;
        var indices = new List<int>();
        var values = new List<float>();

        for (var i = 0; i < dense.Length; i++)
        {
            if (dense[i] != 0)
            {
                indices.Add(i);
                values.Add(dense[i]);
            }
        }

        Dimensions = v.Length;
        Indices = indices.ToArray();
        Values = values.ToArray();
    }

    public SparseVector(string s)
    {
        var parts = s.Split('/', 2);
        var elements = parts[0].Substring(1, parts[0].Length - 2).Split(',');
        var nnz = elements.Length;
        var indices = new int[nnz];
        var values = new float[nnz];

        for (int i = 0; i < nnz; i++)
        {
            var ep = elements[i].Split(':', 2);
            indices[i] = Int32.Parse(ep[0], CultureInfo.InvariantCulture) - 1;
            values[i] = float.Parse(ep[1], CultureInfo.InvariantCulture);
        }

        Dimensions = Int32.Parse(parts[1], CultureInfo.InvariantCulture);
        Indices = indices.ToArray();
        Values = values.ToArray();
    }

    public override string ToString()
    {
        var elements = Indices.ToArray().Zip(Values.ToArray(), (i, v) => string.Concat((i + 1).ToString(CultureInfo.InvariantCulture), ":", v.ToString(CultureInfo.InvariantCulture)));
        return string.Concat("{", string.Join(",", elements), "}/", Dimensions);
    }

    public float[] ToArray()
    {
        var result = new float[Dimensions];
        var indices = Indices.Span;
        var values = Values.Span;

        for (var i = 0; i < indices.Length; i++)
            result[indices[i]] = values[i];

        return result;
    }
}
