using System.Collections.Generic;
using System.Linq;

namespace GrimLib.Archive
{
    public class DirectoryEntry
    {
        private Archive archive;
        DirectoryRecord record;

        bool fullfilled = false;

        public string Name
        {
            get; private set;
        }

        private int flags;

        List<FileEntry> entries;
        List<DirectoryEntry> dirEntries;

        public IList<FileEntry> Files
        {
            get
            {
                if (fullfilled)
                    return entries.AsReadOnly();
                else
                {
                    Fullfill();
                    return Files;
                }
            }
        }

        public IList<DirectoryEntry> Directories
        {
            get
            {
                if (fullfilled)
                    return dirEntries.AsReadOnly();
                else
                {
                    Fullfill();
                    return Directories;
                }
            }
        }

        private void EnumerateFiles()
        {
            entries = new List<FileEntry>();
            for (int i = 0; i < record.Count; i++)
            {
                Record r = record.records[record.records.Keys.ElementAt(i)];
                if ((r.flags & 1) == 0)
                {
                    entries.Add(new FileEntry(r as FileRecord, archive));
                }
            }
        }

        private void Fullfill()
        {
            EnumerateFiles();
            EnumerateDirectories();
            fullfilled = true;
        }

        private void EnumerateDirectories()
        {
            dirEntries = new List<DirectoryEntry>();
            for (int i = 0; i < record.Count; i++)
            {
                Record r = record.records[record.records.Keys.ElementAt(i)];
                if ((r.flags & 1) != 0)
                {
                    dirEntries.Add(new DirectoryEntry(r as DirectoryRecord, this));
                }
            }
        }

        private DirectoryEntry(DirectoryRecord record, DirectoryEntry parent)
        {
            archive = parent.archive;
            Name = record.name;
            flags = record.flags;
            this.record = record;
        }

        internal DirectoryEntry(DirectoryRecord record, Archive archive)
        {
            this.archive = archive;
            Name = record.name;
            flags = record.flags;
            this.record = record;
            Fullfill();
        }
    }
}
