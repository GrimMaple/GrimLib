using System.IO;

namespace GrimLib.Archive
{
    public class FileArchive : Archive
    {
        public FileArchive(string path) : base(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {

        }
    }
}
