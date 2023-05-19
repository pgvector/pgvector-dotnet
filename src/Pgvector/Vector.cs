using System;
using System.Globalization;
using System.Linq;

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
            vec = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", vec.Select(v => v.ToString(CultureInfo.InvariantCulture))), "]");
        }

        public float[] ToArray()
        {
            return vec;
        }
    }
}
