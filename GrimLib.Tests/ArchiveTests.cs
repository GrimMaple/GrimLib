using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using GrimLib.Archive;

namespace GrimLib.Tests
{
    [TestClass]
    public class ArchiveTests
    {
        MemoryArchive ma;

        public ArchiveTests()
        {
            ma = new MemoryArchive();
        }

        [TestMethod]
        public void TestMemoryArchive()
        {
            TestFileCreate();
            TestDirectoryCreate();
            TestSubDirs();
            byte[] data = ma.AsData();
            TestRestore(data);
        }

        private void TestFile(MemoryArchive ma)
        {
            FileEntry fe = ma.GetFile("/nope");
            Assert.AreEqual(fe.Length, 2);
            Assert.AreEqual(fe.Name, "nope");
            Assert.IsFalse(fe.IsCompressed);
        }

        public void TestFileCreate()
        {
            ma.CreateFile("/", "nope", new byte[] { 0, 1 });
            TestFile(ma);
        }

        private void TestDirectory(MemoryArchive ma)
        {
            DirectoryEntry de = ma.GetDirectory("/somename");
            Assert.AreEqual(de.Name, "somename");
            Assert.AreEqual(de.Files.Count, 0);
            Assert.AreEqual(de.Directories.Count, 0);
        }

        public void TestDirectoryCreate()
        {
            ma.CreateDirectory("somename", "/");
            TestDirectory(ma);
        }

        private void CheckSubdirs(MemoryArchive ma)
        {
            DirectoryEntry de = ma.GetDirectory("/subdirs");
            DirectoryEntry subde = de.Directories[0];
            Assert.AreEqual(subde.Name, "sub");
            Assert.AreEqual(subde.Files.Count, 0);
            Assert.AreEqual(subde.Directories.Count, 0);
        }

        public void TestSubDirs()
        {
            ma.CreateDirectory("subdirs", "/");
            ma.CreateDirectory("sub", "/subdirs");
            CheckSubdirs(ma);
        }

        public void TestRestore(byte[] data)
        {
            MemoryArchive ma = new MemoryArchive(data);
            TestFile(ma);
            TestDirectory(ma);
            CheckSubdirs(ma);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void TestExcept()
        {
            MemoryArchive ma = new MemoryArchive();
            ma.GetFile("lolkek");
        }

        [TestCleanup]
        public void Cleanup()
        {

        }
    }
}
