#pragma once

#include "d3dx9.h"

#define DECL_NS_BEGIN_D3DX9()  namespace DereTore { namespace Interop { namespace D3DX9 {
#define DECL_NS_END_D3DX9()    } } }

using namespace System;
using namespace System::Drawing;
using namespace System::Runtime::InteropServices;

DECL_NS_BEGIN_D3DX9()

public ref class D3dx9Utilities abstract sealed {

public:
	static array<BYTE> ^GetDdsTextureFromImage(Bitmap ^bitmap);
	static initonly DWORD DefaultDdsMipLevels = 8;

private:
	static HRESULT GetDdsTextureFromPtr(IntPtr data, UINT32 width, UINT32 height, UINT32 stride, [Out] array<BYTE> ^%ddsFileData);
	static HRESULT GetDdsTextureFromPtr(PVOID data, UINT32 width, UINT32 height, UINT32 stride, [Out] array<BYTE> ^%ddsFileData);

};

DECL_NS_END_D3DX9()
