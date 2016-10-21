using System;
using System.Runtime.InteropServices;

namespace DereTore.Applications.StarlightDirector.Core.Interop {
    internal static class NativeMethods {

        public const string DWMAPI_LIB = "dwmapi.dll";
        public const string DwmGetColorizationParameters_FUNC = "#127";
        public const int DwmGetColorizationParameters_ORD = 127;

        // http://stackoverflow.com/questions/13660976/get-the-active-color-of-windows-8-automatic-color-theme
        [DllImport(DWMAPI_LIB, EntryPoint = DwmGetColorizationParameters_FUNC)]
        public static extern void DwmGetColorizationParameters(out NativeStructures.DWMCOLORIZATIONPARAMS colorParams);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr lpProcOrdinal);

        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint uMilliseconds);

    }
}
