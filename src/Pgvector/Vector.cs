namespace Pgvector;

public static class Vector
{
    public static string Serialize(float[] v)
    {
        return String.Concat("[", String.Join(",", v), "]");
    }

    public static float[] Deserialize(string s)
    {
        return Array.ConvertAll(s.Substring(1, s.Length - 2).Split(","), v => float.Parse(v));
    }
}
