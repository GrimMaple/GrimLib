namespace GrimLib.Archive
{
    public class FileEntry
    {
        private Archive archive;
        private FileRecord record;

        public byte[] Data
        {
            get
            {
                return archive.ReadFile(record);
            }
        }

        public string Name
        {
            get; private set;
        }

        private int flags;

        public bool IsCompressed
        {
            get
            {
                return (flags & (int)FileFlags.Compressed) != 0;
            }
        }

        public long Length
        {
            get
            {
                return Data.Length;
            }
        }

        internal FileEntry(FileRecord record, Archive archive)
        {
            this.archive = archive;
            Name = record.name;
            flags = record.flags;
            this.record = record;
        }
    }
}
