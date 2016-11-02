#pragma once

#include "DirectXTex.h"

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::IO;

using namespace DirectX;

#define DECL_NS_BEGIN_DDSU()  namespace DereTore { namespace Interop { namespace DirectXTex {
#define DECL_NS_END_DDSU()    } } }

DECL_NS_BEGIN_DDSU()

public ref class DdsUtilities abstract sealed {

public:
	static array<BYTE> ^GetDdsTextureFromImage(Bitmap ^bitmap);

	static initonly UINT32 DefaultDdsMipLevels = 8;

private:
	static array<BYTE> ^GetDdsTextureFromPtr(IntPtr data, DWORD width, DWORD height, DWORD stride);

};

DECL_NS_END_DDSU()
