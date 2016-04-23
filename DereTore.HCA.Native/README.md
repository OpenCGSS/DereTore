# DereTore.HCA.Native

This is a C# P/Invoke wrapper for [kawashima](https://github.com/Hozuki/kawashima). This class library
**is platform-dependent**, so please refer to the notes in `NativeMethods.cs`.

There are 3 types of preprocessor directives which should be set before compiling.

- Platform path separator: define `NOT_WINDOWS` on a platform which is not Windows.
- Cross compiling environment: define `USE_CYGWIN`, `USE_MINGW` or define nothing, according to your settings.
- Processor architecture: define `ARCH_X86` or `ARCH_X64` according to your settings.
