#pragma once

#define DECL_NS_BEGIN_MPVR()  namespace DereTore { namespace Interop { namespace PVRTexLib {
#define DECL_NS_END_MPVR()    } } }

#ifndef EXTERN_C
#define EXTERN_C extern "C"
#endif

#ifndef STDCALL
#define STDCALL __stdcall
#endif

#define MPVR_EXPORTS(ret_type) EXTERN_C __declspec(dllexport) ret_type STDCALL

#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
