using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass()]
    public class MaybeTests
    {
        [TestMethod()]
        public void MaybeTest()
        {
            var dict = new Dictionary<int, int>();
            dict[0] = 1;
            dict[2] = 3;
            Assert.IsTrue(dict.Lookup(0).HasValue);
            Assert.IsFalse(dict.Lookup(1).HasValue);
            Assert.AreEqual(dict.Lookup(1).GetOrElse(8), 8);
            Assert.AreEqual(dict.Lookup(2).Map(x => x + 1).GetOrElse(0), 4);
        }
    }
}