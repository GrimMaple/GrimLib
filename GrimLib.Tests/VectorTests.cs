using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GrimLib.Graphics;

namespace GrimLib.Tests
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void TestVectors()
        {
            Vector2D a = new Vector2D(100, 100);
            Vector2D b = new Vector2D(50, 50);

            Assert.AreEqual((a - b).X, 50);
            Assert.AreEqual((a - b).Y, 50);
            Assert.AreEqual((a * 0).X, 0);
            Assert.AreEqual((a * 0).Y, 0);
        }
    }
}
