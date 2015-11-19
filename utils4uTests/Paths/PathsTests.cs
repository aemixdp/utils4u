using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace Tests
{
    [TestClass()]
    public class PathsTests
    {
        [TestMethod()]
        public void BitonicTourTest()
        {
            var pt0 = new Vector3(1, 2);
            var pt1 = new Vector3(2, 5);
            var pt2 = new Vector3(2.5f, 5.5f);
            var pt3 = new Vector3(3, 1);
            var pt4 = new Vector3(4, 4);
            var path = Paths.BitonicTour(new[] { pt1, pt3, pt0, pt4, pt2 });
            Assert.AreEqual(path[0], pt2);
            Assert.AreEqual(path[1], pt1);
            Assert.AreEqual(path[2], pt0);
            Assert.AreEqual(path[3], pt3);
            Assert.AreEqual(path[4], pt4);
        }
    }
}