//#define NOT_WINDOWS
#define CROSS_USE_CYGWIN
#define ARCH_X86

using System;
using System.Runtime.InteropServices;

namespace DereTore.HCA.Native {
    internal static class NativeMethods {
        // Platform path separator directive. Hmm, its meaning seems quite clear.
#if NOT_WINDOWS
        private const string PathSep = "/";
#else
        private const string PathSep = "\\";
#endif
        // Cross-platform compiling directive. To compromise with *nix systems, you may use Cygwin or MinGW environments to compile kawashima.
#if CROSS_USE_CYGWIN
        private const string LibCrossPrefix = "cyg";
#elif CROSS_USE_MINGW
        private const string LibCrossPrefix = "lib";
#else
        private const string LibCrossPrefix = "";
#endif
        // Processor architecture directive.
#if ARCH_X64
        private const string LibArchPrefix = "lib" + PathSep+ "x64" + PathSep;
#elif ARCH_X86
        private const string LibArchPrefix = "lib" + PathSep + "x86" + PathSep;
#else
#error Processor architecture is not supported. Please define ARCH_X64 or ARCH_X86.
#endif
        private const string DllName = "kawashima";

        // For example, a kawashima library compiled under Cygwin, targetted for x64 systems, should be placed at:
        // {APP_PATH}/lib/x64/cygkawashima.dll

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsOpenFile([MarshalAs(UnmanagedType.LPStr)] string pFileName, out IntPtr ppHandle);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsOpenBuffer([MarshalAs(UnmanagedType.LPArray)] byte[] pData, uint dwDataSize, [MarshalAs(UnmanagedType.Bool)] bool bClone, out IntPtr ppHandle);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsSetParamI32(IntPtr hDecode, [MarshalAs(UnmanagedType.U4)] KsParamType dwParamType, uint dwParam);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsSetParamI64(IntPtr hDecode, [MarshalAs(UnmanagedType.U4)] KsParamType dwParamType, ulong qwParam);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsBeginDecode(IntPtr hDecode);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsGetWaveHeader(IntPtr hDecode, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, ref uint dataSize);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsDecodeData(IntPtr hDecode, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, ref uint dataSize);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsEndDecode(IntPtr hDecode);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsCloseHandle(IntPtr hDecode);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsGetHcaInfo(IntPtr hDecode, out HcaInfo info);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool KsIsActiveHandle(IntPtr hDecode);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool KsIsHcaCheckPassed(IntPtr hDecode);

        [DllImport(LibArchPrefix + LibCrossPrefix + DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern KsResult KsHasMoreData(IntPtr hDecode, [MarshalAs(UnmanagedType.Bool)] out bool bHasMore);
    }
}