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

    // caller must ensure:
    // 1. indices are sorted, unique, >= 0, and < dimensions
    // 2. values does not contain zeros
    public SparseVector(int dimensions, ReadOnlyMemory<int> indices, ReadOnlyMemory<float> values)
    {
        if (indices.Length != values.Length)
        {
            throw new ArgumentException("indices and values must be the same length");
        }

        Dimensions = dimensions;
        Indices = indices;
        Values = values;
    }

    public SparseVector(ReadOnlyMemory<float> v)
    {
        var dense = v.Span;
        var count = 0;
        var capacity = 4;
        var indices = new int[capacity];
        var values = new float[capacity];

        for (var i = 0; i < dense.Length; i++)
        {
            if (dense[i] != 0)
            {
                if (count == capacity)
                {
                    capacity = capacity >= dense.Length / 2 ? dense.Length : capacity * 2;
                    Array.Resize(ref indices, capacity);
                    Array.Resize(ref values, capacity);
                }

                indices[count] = i;
                values[count] = dense[i];
                count++;
            }
        }

        Dimensions = v.Length;
        Indices = new ReadOnlyMemory<int>(indices, 0, count);
        Values = new ReadOnlyMemory<float>(values, 0, count);
    }

    public SparseVector(IDictionary<int, float> dictionary, int dimensions)
    {
        var count = 0;
        var capacity = dictionary.Count;
        var indices = new int[capacity];
        var values = new float[capacity];

        foreach (var e in dictionary)
        {
            if (e.Value != 0)
            {
                indices[count] = e.Key;
                values[count] = e.Value;
                count++;
            }
        }

        Array.Sort(indices, values, 0, count);

        Dimensions = dimensions;
        Indices = new ReadOnlyMemory<int>(indices, 0, count);
        Values = new ReadOnlyMemory<float>(values, 0, count);
    }

    public SparseVector(string s)
    {
        var parts = s.Split(['/'], 2);
        var elements = parts[0].Substring(1, parts[0].Length - 2).Split(',');
        var nnz = elements.Length;
        var indices = new int[nnz];
        var values = new float[nnz];

        for (int i = 0; i < nnz; i++)
        {
            var ep = elements[i].Split([':'], 2);
            indices[i] = Int32.Parse(ep[0], CultureInfo.InvariantCulture) - 1;
            values[i] = float.Parse(ep[1], CultureInfo.InvariantCulture);
        }

        Dimensions = Int32.Parse(parts[1], CultureInfo.InvariantCulture);
        Indices = indices;
        Values = values;
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

    public bool Equals(SparseVector? other)
        => other is not null && Dimensions == other.Dimensions && Indices.Span.SequenceEqual(other.Indices.Span) && Values.Span.SequenceEqual(other.Values.Span);

    public override bool Equals(object? obj)
        => obj is SparseVector vector && Equals(vector);

    public static bool operator ==(SparseVector? x, SparseVector? y)
        => (x is null && y is null) || (x is not null && x.Equals(y));

    public static bool operator !=(SparseVector? x, SparseVector? y) => !(x == y);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Dimensions);

        var indices = Indices.Span;
        for (var i = 0; i < indices.Length; i++)
            hashCode.Add(indices[i]);

        var values = Values.Span;
        for (var i = 0; i < values.Length; i++)
            hashCode.Add(values[i]);

        return hashCode.ToHashCode();
    }
}
