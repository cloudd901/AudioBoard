using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AudioBoard
{
    public class IniFile
    {
        private static readonly string _EXE = Assembly.GetExecutingAssembly().GetName().Name;

        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? _EXE + ".ini").FullName;
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }
        }

        private string Path { get; }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? _EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? _EXE);
        }

        public List<string> GetKeys(string Sec)
        {
            List<string> textList = new List<string>();
            string text = File.ReadAllText(Path);
            string Section = "[" + Sec + "]";
            if (text.Contains(Section))
            {
                int loc1 = text.IndexOf(Section, StringComparison.OrdinalIgnoreCase);
                if (loc1 < 0)
                {
                    loc1 = 0;
                }

                int loc2 = text.IndexOf("[", loc1 + 1, StringComparison.OrdinalIgnoreCase);
                if (loc2 < 0)
                {
                    loc2 = text.Length;
                }

                text = text[loc1..loc2];
                text = text.Replace(Section, string.Empty);
                text = text.Replace("\r\n", "|");
                string[] textarr = text.Split('|');
                for (int i = 0; i < textarr.Length; i++)
                {
                    if (textarr[i].Contains("="))
                    {
                        textList.Add(textarr[i].Split('=')[0]);
                    }
                }
            }

            return textList;
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        public string Read(string key, string section = null)
        {
            StringBuilder RetVal = new StringBuilder(255);
            _ = NativeMethods.GetPrivateProfileString(section ?? _EXE, key, string.Empty, RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string key, string value, string section = null)
        {
            NativeMethods.WritePrivateProfileString(section ?? _EXE, key, value, Path);
        }
    }
}