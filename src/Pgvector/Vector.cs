namespace Pgvector;

public class Vector
{
    private float[] vec;

    public Vector(float[] v)
    {
        vec = v;
    }

    public Vector(String s)
    {
        vec = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(","), v => float.Parse(v));
    }

    public override string ToString()
    {
        return String.Concat("[", String.Join(",", vec), "]");
    }

    public float[] ToArray()
    {
        return vec;
    }
}
