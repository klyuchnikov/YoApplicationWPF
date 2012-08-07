using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace YoApplication
{
    public static class FontInstall
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, // handle to destination window 
        uint Msg, // message 
        int wParam, // first message parameter 
        int lParam // second message parameter 
        );

        [DllImport("gdi32")]
        public static extern int AddFontResource(string lpFileName);
    }
}
