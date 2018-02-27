using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace GrimLib.Configuration
{
    public class Config
    {
        private void SetOwnValue(string name, string val)
        {
            Type[] accepted = { typeof(int), typeof(long), typeof(float), typeof(double), typeof(string), typeof(bool) };
            PropertyInfo inf = GetType().GetRuntimeProperty(name);
            int t = Array.IndexOf(accepted, inf.PropertyType);
            switch (t)
            {
                case 0:
                    inf.SetValue(this, int.Parse(val));
                    break;
                case 1:
                    inf.SetValue(this, long.Parse(val));
                    break;
                case 2:
                    inf.SetValue(this, float.Parse(val, CultureInfo.InvariantCulture));
                    break;
                case 3:
                    inf.SetValue(this, double.Parse(val, CultureInfo.InvariantCulture));
                    break;
                case 4:
                    inf.SetValue(this, val);
                    break;
                case 5:
                    inf.SetValue(this, bool.Parse(val));
                    break;
            }
        }

        /// <summary>
        /// Load configuration from given file
        /// </summary>
        /// <param name="stream"></param>
        public void Load(Stream stream)
        {
            Type t = GetType();
            ConfigParser p = new ConfigParser(stream);
            Dictionary<string, string> d = p.Parse();
            foreach (string key in d.Keys)
            {
                SetOwnValue(key, d[key]);
            }
        }

        /// <summary>
        /// Save configuration into given file (forces overwrite)
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            // Epic workaround of StreamWriter writing floats as X,X in some Cultures
            CultureInfo ci = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            StreamWriter sw = new StreamWriter(stream);
            IEnumerable<PropertyInfo> infos = GetType().GetRuntimeProperties();

            foreach(PropertyInfo info in infos)
            {
                Attribute attr = info.GetCustomAttribute<OptionAttribute>();
                if (attr == null)
                    continue;
                string name = info.Name;
                if (info.GetValue(this) == null)
                    continue;
                string value;
                if (info.PropertyType == typeof(string))
                    value = string.Format("\"{0}\"", info.GetValue(this).ToString());
                else
                    value = info.GetValue(this).ToString();
                sw.WriteLine("{0}: {1}", name, value);
            }

            sw.Dispose();
            CultureInfo.CurrentCulture = ci;

            /*for (int i = 0; i < infos.Length; i++)
            {
                Attribute attr = infos[i].GetCustomAttribute(typeof(OptionAttribute));
                if (attr == null)
                    continue;

                string name = infos[i].Name;
                if (infos[i].GetValue(this) == null)
                    continue;
                string value = infos[i].GetValue(this).ToString();
                if (infos[i].PropertyType == typeof(string))
                    value = string.Format("\"{0}\"", infos[i].GetValue(this).ToString());
                sw.WriteLine("{0}: {1}", name, value.ToString());
            }
            sw.Close();
            Thread.CurrentThread.CurrentCulture = ci;*/
        }
    }
}
