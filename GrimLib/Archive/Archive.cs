using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimLib.Archive
{
    public abstract class Archive
    {
        long end;

        protected Stream baseStream;

        DirectoryRecord root;

        BinaryReader br;
        BinaryWriter bw;

        List<string> pathes = new List<string>();

        public Archive(Stream s)
        {
            baseStream = s;
            root = new DirectoryRecord("/");
            br = new BinaryReader(s);
            bw = new BinaryWriter(s);
            if (s.Length < 16)
            {
                baseStream.Write(new byte[] { 0, 0, 0, 0, 0x0c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 16);
            }
            ReadFileSystem();
        }

        private Record GetByPath(string path)
        {
            Record ret = root;
            if (path == null)
                return root;
            string[] splits = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < splits.Length; i++)
                {
                    ret = (ret as DirectoryRecord)[splits[i]];
                }
                return ret;
            }
            catch
            {
                throw new Exception();
            }
        }

        public void CreateDirectory(string name, string path)
        {
            DirectoryRecord n = new DirectoryRecord(name);
            n.flags = 1;
            (GetByPath(path) as DirectoryRecord).Add(n);
            return;
        }

        private string CombinePath()
        {
            if (pathes.Count == 0)
                return null;
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < pathes.Count; i++)
            {
                bld.Append(pathes[i]);
                bld.Append("/");
            }
            return bld.ToString();
        }

        private void AddDirectory(string name, DirectoryRecord record)
        {
            DirectoryRecord rec = GetByPath(CombinePath()) as DirectoryRecord;
            rec.Add(record);
        }

        private void AddFile(string name, FileRecord record)
        {
            DirectoryRecord rec = GetByPath(CombinePath()) as DirectoryRecord;
            rec.Add(record);
        }

        private void ParseNext()
        {
            int flags = br.ReadInt32();
            string name = br.ReadString();
            name = name.Replace("\0", "");
            if ((flags & 1) == 0)
            {
                long position = br.ReadInt64();
                long length = br.ReadInt64();
                FileRecord rec = new FileRecord(name, position);
                rec.length = length;
                AddFile(name, rec);
            }
            else
            {
                DirectoryRecord rec = new DirectoryRecord(name);
                AddDirectory(name, rec);
                pathes.Add(name);
                ParseDirectory();
                pathes.RemoveAt(pathes.Count - 1);
            }
        }

        private void ParseDirectory()
        {
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ParseNext();
            }
        }

        public void CreateFile(string path, string name, byte[] data)
        {
            DirectoryRecord rec = GetByPath(path) as DirectoryRecord;
            FileRecord fr = new FileRecord(name, end);
            fr.length = data.Length;
            rec.Add(fr);
            baseStream.Seek(end, SeekOrigin.Begin);
            baseStream.Write(data, 0, data.Length);
            end += data.Length;
        }

        public FileEntry GetFile(string path)
        {
            FileRecord record = GetByPath(path) as FileRecord;
            return new FileEntry(record, this);
        }

        public DirectoryEntry GetDirectory(string path)
        {
            DirectoryRecord record = GetByPath(path) as DirectoryRecord;
            return new DirectoryEntry(record, this);
        }

        internal byte[] ReadFile(FileRecord rec)
        {
            baseStream.Seek(rec.location, SeekOrigin.Begin);
            byte[] ret = new byte[rec.length];
            baseStream.Read(ret, 0, (int)rec.length);
            return ret;
        }

        protected void ReadFileSystem()
        {
            baseStream.Seek(4, SeekOrigin.Begin);
            end = br.ReadInt64();
            baseStream.Seek(end, SeekOrigin.Begin);
            ParseDirectory();
        }

        private void SaveRecord(Record rec)
        {
            bw.Write(rec.flags);
            bw.Write(rec.name);
            if ((rec.flags & 1) == 1)
            {
                DirectoryRecord dr = rec as DirectoryRecord;
                bw.Write(dr.Count);
                foreach (string key in dr.records.Keys)
                {
                    SaveRecord(dr.records[key]);
                }
            }
            else
            {
                bw.Write((rec as FileRecord).location);
                bw.Write((rec as FileRecord).length);
            }
        }

        public void Save()
        {
            baseStream.Seek(4, SeekOrigin.Begin);
            bw.Write(end);
            baseStream.Seek(end, SeekOrigin.Begin);
            bw.Write(root.Count);
            foreach (string r in root.records.Keys)
            {
                SaveRecord(root[r]);
            }
            //SaveRecord(root);
            /*foreach (string key in records.Keys)
            {
                SaveRecord(records[key]);
            }*/
        }
    }
}
