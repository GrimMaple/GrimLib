using System.Collections.Generic;

namespace GrimLib.Archive
{
    class DirectoryRecord : Record
    {
        public Dictionary<string, Record> records;

        public Record this[string name]
        {
            get
            {
                return records[name];
            }
        }

        public int Count
        {
            get
            {
                return records.Keys.Count;
            }
        }

        public DirectoryRecord(string name) : base(name)
        {
            flags = 1;
            records = new Dictionary<string, Record>();
        }

        public void Add(Record record)
        {
            records.Add(record.name, record);
        }
    }
}
