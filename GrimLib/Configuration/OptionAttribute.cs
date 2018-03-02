using System;

namespace GrimLib.Configuration
{
    /// <summary>
    /// Dummy attribute to test either one property should be saved
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        public string Name { get; set; }

        public OptionAttribute()
        {
            Name = null;
        }

        public OptionAttribute(string name)
        {
            Name = name;
        }
    }
}
