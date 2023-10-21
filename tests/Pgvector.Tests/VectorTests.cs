using Pgvector;

namespace Pgvector.Tests;

public class VectorTests
{
    [Fact]
    public void StringConstructor()
    {
        var v = new Vector("[1,2,3]");
        Assert.Equal("[1,2,3]", v.ToString());
    }

    [Fact]
    public void ArrayConstructor()
    {
        var v = new Vector(new float[] { 1, 2, 3 });
        Assert.Equal(new float[] { 1, 2, 3 }, v.ToArray());
    }

    [Fact]
    public void Equal()
    {
        var a = new Vector(new float[] { 1, 1, 1 });
        var b = new Vector(new float[] { 1, 1, 1 });
        var c = new Vector(new float[] { 1, 2, 3 });

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);

        Assert.True(a == b);
        Assert.False(a == c);

        Assert.False(a != b);
        Assert.True(a != c);

        Assert.False(a == null);
        Assert.False(null == a);
        Assert.True((Vector?)null == null);
    }
}
