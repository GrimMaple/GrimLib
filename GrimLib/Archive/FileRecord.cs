namespace GrimLib.Archive
{
    class FileRecord : Record
    {
        public long location;
        public long length;

        public FileRecord(string name, long loc) : base(name)
        {
            location = loc;
        }
    }
}
