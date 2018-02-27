using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrimLib.Tests
{
    using System.IO;
    using Util;
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void TestConfig()
        {
            TestConfiguration config = new TestConfiguration();
            MemoryStream ms = new MemoryStream();
            config.Save(ms);
            byte[] data = ms.ToArray();
            ms = new MemoryStream(data);
            TestConfiguration read = new TestConfiguration(true);
            read.Load(ms);
            Assert.AreEqual(1, config.CompareTo(read));
        }
    }
}
