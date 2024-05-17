using Pgvector;

namespace Pgvector.Tests;

public class SparseVectorTests
{
    [Fact]
    public void ArrayConstructor()
    {
        var v = new SparseVector(new float[] { 1, 0, 2, 0, 3, 0 });
        Assert.Equal(new float[] { 1, 0, 2, 0, 3, 0 }, v.ToArray());
    }
}
