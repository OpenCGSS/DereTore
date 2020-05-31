using System.Runtime.InteropServices;

namespace DereTore.Interop.OS {
    public static class NativeMethods {

        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        public static extern uint timeGetTime();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplaySettings([MarshalAs(UnmanagedType.LPTStr)]string lpszDeviceName, int iModeNum, out NativeStructures.DEVMODE lpDevMode);

    }
}
