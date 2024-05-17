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
