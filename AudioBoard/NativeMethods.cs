using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AudioBoard
{
    internal static class NativeMethods
    {
        public const int HT_CAPTION = 0x2;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        internal const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        internal const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImportAttribute("kernel32", CharSet = CharSet.Unicode)]
        internal static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImportAttribute("kernel32", CharSet = CharSet.Unicode)]
        internal static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
    }
}