#pragma once

#ifndef EXTERN_C
#define EXTERN_C extern "C"
#endif

#ifndef DLLEXPORT
#define DLLEXPORT __declspec(dllexport)
#endif

#ifndef STDCALL
#define STDCALL __stdcall
#endif

#define MPVR_EXPORTS(ret_type) EXTERN_C DLLEXPORT ret_type STDCALL

#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
