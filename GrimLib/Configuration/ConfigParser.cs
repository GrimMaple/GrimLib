using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimLib.Configuration
{
    internal class ConfigParser
    {
        private Dictionary<string, string> result;
        StreamReader sr;
        ParseState state = ParseState.None;

        bool isString = false;

        StringBuilder name;
        StringBuilder value;

        private void NoneState(char t)
        {
            if (t == ':')
                throw new Exception("Invalid file");
            if (t != ' ')
            {
                name.Append(t);
                state = ParseState.Name;
            }
        }

        private void NameState(char t)
        {
            if (t == ' ')
            {
                state = ParseState.PreDots;
                return;
            }
            if (t == ':')
            {
                state = ParseState.Dots;
                return;
            }
            name.Append(t);
        }

        private void PreDotsState(char t)
        {
            if (t != ' ' && t != ':')
                throw new Exception("Invalid file");
            if (t == ':')
                state = ParseState.Dots;
        }

        private void DotsState(char t)
        {
            if (t == ' ')
                return;
            if (t == '"')
            {
                isString = true;
                state = ParseState.Value;
                return;
            }
            state = ParseState.Value;
            value.Append(t);
        }

        private void ValueState(char t)
        {
            if (t == ' ' && !isString)
            {
                state = ParseState.AfterValue;
                return;
            }
            if (t == '"' && isString)
            {
                state = ParseState.AfterValue;
                isString = false;
                return;
            }
            value.Append(t);
        }

        private void AfterValueState(char t)
        {
            if (t != ' ')
                throw new Exception("Invalid file");
        }

        private void AddValue()
        {
            string n = name.ToString(), v = value.ToString();
            if (n == "" && v == "")
                return;
            if (n == "" || v == "")
                throw new Exception("Invalid file");

            result.Add(n, v);
        }

        private void ParseLine(string line)
        {
            state = ParseState.None;
            name = new StringBuilder(256);
            value = new StringBuilder(256);
            int i = 0;
            while (i < line.Length)
            {
                char t = line[i];
                if (t == '#')
                    return;
                switch (state)
                {
                    case ParseState.None:
                        NoneState(t);
                        break;
                    case ParseState.Name:
                        NameState(t);
                        break;
                    case ParseState.PreDots:
                        PreDotsState(t);
                        break;
                    case ParseState.Dots:
                        DotsState(t);
                        break;
                    case ParseState.Value:
                        ValueState(t);
                        break;
                    case ParseState.AfterValue:
                        AfterValueState(t);
                        break;
                }
                i++;
            }
            AddValue();
        }

        public ConfigParser(Stream stream)
        {
            sr = new StreamReader(stream);
            result = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Parse()
        {
            while (!sr.EndOfStream)
            {
                ParseLine(sr.ReadLine());
            }
            sr.Dispose();
            return result;
        }
    }
}
