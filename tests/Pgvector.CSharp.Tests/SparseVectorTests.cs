using Pgvector;

namespace Pgvector.Tests;

public class SparseVectorTests
{
    [Fact]
    public void StringConstructor()
    {
        var v = new SparseVector("{1:1,3:2,5:3}/6");
        Assert.Equal("{1:1,3:2,5:3}/6", v.ToString());
        Assert.Equal(new float[] { 1, 0, 2, 0, 3, 0 }, v.ToArray());
    }

    [Fact]
    public void ArrayConstructor()
    {
        var v = new SparseVector(new float[] { 1, 0, 2, 0, 3, 0 });
        Assert.Equal(new float[] { 1, 0, 2, 0, 3, 0 }, v.ToArray());
    }
}