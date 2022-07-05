using NUnit.Framework;
using System.Linq;
using Volight.Allocators;
using Volight.Allocators.Internal;

namespace Tests;
public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var ra = new RangeAlloc<int>(0, 100);
        Assert.IsTrue(ra.Alloc(0, 1));
        Assert.IsTrue(ra.Alloc(1, 1));
    }

    [Test]
    public void Test2()
    {
        var ra = new RangeAlloc<int>(0, 100);
        Assert.IsTrue(ra.Alloc(99, 1));
        Assert.IsTrue(ra.Alloc(98, 1));
    }

    [Test]
    public void Test3()
    {
        var ra = new RangeAlloc<int>(0, 100);
        Assert.IsTrue(ra.Alloc(5, 1));
        Assert.IsTrue(ra.Alloc(7, 1));
    }

    [Test]
    public void Test4()
    {
        var ra = new RangeAlloc<int>(0, 100);
        Assert.IsTrue(ra.Alloc(25, 50));
    }

    [Test]
    public void TestNumsAdd()
    {
        foreach (var _ in Enumerable.Range(0, 100))
        {
            var r = Nums.Add(1, 2);
            Assert.AreEqual(3, r);
        }
    }
}