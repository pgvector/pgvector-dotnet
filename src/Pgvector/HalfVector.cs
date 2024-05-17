using System;
using System.Globalization;
using System.Linq;

namespace Pgvector;

public class HalfVector : IEquatable<HalfVector>
{
    public ReadOnlyMemory<Half> Memory { get; }

    public HalfVector(ReadOnlyMemory<Half> v)
        => Memory = v;

    public HalfVector(string s)
        => Memory = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => Half.Parse(v, CultureInfo.InvariantCulture));

    public override string ToString()
        => string.Concat("[", string.Join(",", Memory.ToArray().Select(v => v.ToString(CultureInfo.InvariantCulture))), "]");

    public Half[] ToArray()
        => Memory.ToArray();

    public bool Equals(HalfVector? other)
        => other is not null && Memory.Span.SequenceEqual(other.Memory.Span);

    public override bool Equals(object? obj)
        => obj is HalfVector vector && Equals(vector);

    public static bool operator ==(HalfVector? x, HalfVector? y)
        => (x is null && y is null) || (x is not null && x.Equals(y));

    public static bool operator !=(HalfVector? x, HalfVector? y) => !(x == y);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        var span = Memory.Span;

        for (var i = 0; i < span.Length; i++)
            hashCode.Add(span[i]);

        return hashCode.ToHashCode();
    }
}
