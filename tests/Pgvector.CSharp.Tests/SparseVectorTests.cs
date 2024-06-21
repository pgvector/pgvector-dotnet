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

    [Fact]
    public void DictionaryConstructor()
    {
        var dictionary = new Dictionary<int, float>();
        dictionary.Add(2, 2);
        dictionary.Add(4, 3);
        dictionary.Add(0, 1);
        dictionary.Add(3, 0);
        var v = new SparseVector(dictionary, 6);
        Assert.Equal(new float[] { 1, 0, 2, 0, 3, 0 }, v.ToArray());
        Assert.Equal(new int[] { 0, 2, 4 }, v.Indices.ToArray());
    }
}
