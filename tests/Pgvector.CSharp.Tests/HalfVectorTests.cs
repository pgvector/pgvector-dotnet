using Pgvector;

namespace Pgvector.Tests;

public class HalfVectorTests
{
    [Fact]
    public void StringConstructor()
    {
        var v = new HalfVector("[1,2,3]");
        Assert.Equal("[1,2,3]", v.ToString());
    }

    [Fact]
    public void ArrayConstructor()
    {
        var v = new HalfVector(new Half[] { (Half)1, (Half)2, (Half)3 });
        Assert.Equal(new Half[] { (Half)1, (Half)2, (Half)3 }, v.ToArray());
    }

    [Fact]
    public void Equal()
    {
        var a = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)1 });
        var b = new HalfVector(new Half[] { (Half)1, (Half)1, (Half)1 });
        var c = new HalfVector(new Half[] { (Half)1, (Half)2, (Half)3 });

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);

        Assert.True(a == b);
        Assert.False(a == c);

        Assert.False(a != b);
        Assert.True(a != c);

        Assert.False(a == null);
        Assert.False(null == a);
        Assert.True((HalfVector?)null == null);
    }
}
