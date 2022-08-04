using NUnit.Framework;
using System.Diagnostics;
using Volight.Allocators;

var ra = new RangeAlloc<int>(0, 100);
//Assert.IsTrue(ra.Alloc(10, 1));
//Assert.IsTrue(ra.Alloc(8, 1));
//Assert.IsTrue(ra.Alloc(6, 1));
Assert.IsTrue(ra.Alloc(11, 1));
Assert.IsTrue(ra.Alloc(13, 1));
Assert.IsTrue(ra.Alloc(15, 1));
Assert.IsTrue(ra.Alloc(17, 1));
Assert.IsTrue(ra.Alloc(19, 1));
Assert.IsTrue(ra.Alloc(1, 1));

Debug.WriteLine("");
Debug.WriteLine(ra.DebugPrint());
