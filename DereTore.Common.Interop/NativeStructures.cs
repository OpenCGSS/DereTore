using System.Runtime.InteropServices;

namespace DereTore.Interop {
    public static class NativeStructures {
        [StructLayout(LayoutKind.Sequential)]
        public struct DWMCOLORIZATIONPARAMS {
            public uint ColorizationColor;
            public uint ColorizationAfterglow;
            public uint ColorizationColorBalance;
            public uint ColorizationAfterglowBalance;
            public uint ColorizationBlurBalance;
            public uint ColorizationGlassReflectionIntensity;
            public uint ColorizationOpaqueBlend;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEVMODE {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NativeConstants.CCHDEVICENAME)]
            public char[] dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public uint dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NativeConstants.CCHFORMNAME)]
            public char[] dmFormName;
            public short dmUnusedPadding;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

    }
}
