using System;

namespace Pgvector
{
    public class Vector
    {
        private float[] vec;

        public Vector(float[] v)
        {
            vec = v;
        }

        public Vector(string s)
        {
            vec = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v));
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", vec), "]");
        }

        public float[] ToArray()
        {
            return vec;
        }
    }
}