using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Core.Interop;

namespace DereTore.Applications.StarlightDirector {
    public static class UIHelper {

        public static void RegisterWndProc(this Window window, HwndSourceHook hook) {
            var interopHelper = new WindowInteropHelper(window);
            var hWnd = interopHelper.Handle;
            if (hWnd == IntPtr.Zero) {
                throw new InvalidOperationException("Cannot get the window handle.");
            }
            var hwndSource = HwndSource.FromHwnd(hWnd);
            if (hwndSource == null) {
                throw new InvalidOperationException("Cannot construct a HwndSource from known window handle.");
            }
            hwndSource.AddHook(hook);
        }

        public static SolidColorBrush GetWindowColorizationBrush() {
            return new SolidColorBrush(GetWindowColorizationColor());
        }

        public static SolidColorBrush GetWindowColorizationBrush(bool opaque) {
            return new SolidColorBrush(GetWindowColorizationColor(opaque));
        }

        public static Color GetWindowColorizationColor() {
            return GetWindowColorizationColor(true);
        }

        public static Color GetWindowColorizationColor(bool opaque) {
            if (!GetColorizationAvailability()) {
                return SystemColors.HighlightColor;
            }
            NativeStructures.DWMCOLORIZATIONPARAMS colorParams;
            NativeMethods.DwmGetColorizationParameters(out colorParams);
            var alpha = (byte)(opaque ? 255 : (colorParams.ColorizationColor >> 24) & 0xff);
            var red = (byte)((colorParams.ColorizationColor >> 16) & 0xff);
            var green = (byte)((colorParams.ColorizationColor >> 8) & 0xff);
            var blue = (byte)(colorParams.ColorizationColor & 0xff);
            return Color.FromArgb(alpha, red, green, blue);
        }

        private static bool GetColorizationAvailability() {
            var hLibrary = NativeMethods.LoadLibrary(NativeMethods.DWMAPI_LIB);
            if (hLibrary == IntPtr.Zero) {
                return false;
            }
            var procAddress = NativeMethods.GetProcAddress(hLibrary, (IntPtr)NativeMethods.DwmGetColorizationParameters_ORD);
            var result = procAddress != IntPtr.Zero;
            NativeMethods.FreeLibrary(hLibrary);
            return result;
        }

    }
}
