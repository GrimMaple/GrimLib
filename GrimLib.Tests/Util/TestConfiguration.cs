using GrimLib.Configuration;
using System;

namespace GrimLib.Tests.Util
{
    class TestConfiguration : Config, IComparable
    {
        [Option]
        public string Name { get; set; }

        [Option]
        public int Count { get; set; }

        [Option]
        public long CountLong { get; set; }

        [Option]
        public float SomeFloat { get; set; }

        [Option]
        public double SomeDouble { get; set; }

        [Option]
        public bool SomeFlag { get; set; }

        [Option("SomeRandName")]
        public int SomeInt { get; set; }

        public TestConfiguration(bool empty = false)
        {
            if (empty)
                return;
            Name = "Hello, world";
            Count = 42;
            CountLong = 9001;
            SomeFloat = 3.1415f;
            SomeDouble = 2.71;
            SomeFlag = true;
            SomeInt = 1337;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is TestConfiguration))
                return -1;
            TestConfiguration t = obj as TestConfiguration;
            bool result = true;
            result &= Name == t.Name;
            result &= Count == t.Count;
            result &= SomeFloat == t.SomeFloat;
            result &= SomeDouble == t.SomeDouble;
            result &= SomeFlag == t.SomeFlag;
            result &= SomeInt == t.SomeInt;
            return result ? 1 : -1;
        }
    }
}
