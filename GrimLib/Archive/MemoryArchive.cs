using System.IO;

namespace GrimLib.Archive
{
    public class MemoryArchive : Archive
    {
        public MemoryArchive() : base(new MemoryStream())
        {

        }

        public MemoryArchive(byte[] data) : base(new MemoryStream(data))
        {

        }

        public byte[] AsData()
        {
            Save();
            return (baseStream as MemoryStream).ToArray();
        }
    }
}
