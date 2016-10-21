using System;
using System.Runtime.InteropServices;

namespace DereTore.Applications.ScoreEditor.Interop {
    internal static class NativeMethods {
        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint uMilliseconds);

    }
}
