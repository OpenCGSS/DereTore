#pragma once
#include "mpvr_header.h"
#include <Windows.h>

using namespace pvrtexture;

using namespace System;
using namespace System::Drawing;

DECL_NS_BEGIN_MPVR()

struct MpvrEncodePreset {
	PixelType pixelType;
	ECompressorQuality quality;
	LPCSTR extension;
	DWORD mipLevels;
};

public ref class PvrUtilities abstract sealed {

public:
	static array<BYTE> ^GetPvrTextureFromImage(Bitmap ^bitmap);
	static array<BYTE> ^GetPvrTextureFromImage(Bitmap ^bitmap, bool withHeader);

	static initonly DWORD DefaultDdsMipLevels = 8;
	static initonly DWORD DefaultPvrMipLevels = 7;

private:
	static array<BYTE> ^GetTextureWithHeaderFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height, DWORD stride, MpvrEncodePreset &preset);
	static array<BYTE> ^GetTextureWithHeaderFromImage(Bitmap ^bitmap, MpvrEncodePreset &preset);

	static array<BYTE> ^GetDdsTextureWithHeaderFromImage(Bitmap ^bitmap);
	static array<BYTE> ^GetPvrTextureWithHeaderFromImage(Bitmap ^bitmap);

};

DECL_NS_END_MPVR()

MPVR_EXPORTS(VOID)  MpvrCompressPvrTextureFrom32bppArgb(PVOID pBitmapData, DWORD dwWidth, DWORD dwHeight, DWORD dwStride, DWORD dwMipLevels, OUT PBYTE *pCompressedTexture, OUT PDWORD pdwTextureDataSize);
MPVR_EXPORTS(PBYTE) MpvrCompressPvrTexture(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, DWORD dwStride, UINT32 dwMipLevels, PixelType eInputPixelType, BOOL bIsPremultiplied, OUT PDWORD *ppdwDataSizes);
