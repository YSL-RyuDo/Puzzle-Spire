using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
namespace PuzzleSpire.Hotel
{
    public static class HotelPngPicker
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private sealed class OpenFile
        {
            public int size;
            public IntPtr owner, instance;
            public string filter = "PNG 이미지\0*.png\0\0";
            public IntPtr customFilter;
            public int maxCustomFilter, filterIndex = 1;
            public IntPtr file;
            public int maxFile = 32768;
            public IntPtr fileTitle;
            public int maxFileTitle;
            public string initialDir, title = "이야기에 사용할 PNG 그림 선택";
            public int flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            public short fileOffset, fileExtension;
            public string defaultExtension = "png";
            public IntPtr customData, hook, template, reserved;
            public int reservedNumber, flagsEx;
        }
        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetOpenFileNameW([In, Out] OpenFile file);
        [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
        public static string Open()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string directory = Directory.GetCurrentDirectory();
            IntPtr buffer = Marshal.AllocHGlobal(32768 * 2); Marshal.WriteInt16(buffer, 0);
            try
            {
                var dialog = new OpenFile { owner = GetActiveWindow(), file = buffer }; dialog.size = Marshal.SizeOf(dialog);
                return GetOpenFileNameW(dialog) ? Marshal.PtrToStringUni(buffer) : null;
            }
            finally { Marshal.FreeHGlobal(buffer); Directory.SetCurrentDirectory(directory); }
#else
            throw new InvalidOperationException("PNG 경로를 입력한 뒤 가져오기를 눌러 주세요.");
#endif
        }
    }
}
