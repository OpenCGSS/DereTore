#pragma once
#include "mpvr_header.h"
#include <Windows.h>

using namespace pvrtexture;

DECL_NS_BEGIN_MPVR()

struct MpvrEncodePreset {
	PixelType pixelType;
	ECompressorQuality quality;
	LPCSTR extension;
	DWORD mipLevels;
};

public ref class PvrUtilities abstract sealed {

public:
	static array<BYTE> ^GetPvrTextureFromImage(System::Drawing::Bitmap ^bitmap);
	static array<BYTE> ^GetDdsTextureFromImage(System::Drawing::Bitmap ^bitmap);

	static initonly DWORD DefaultPvrMipLevels = 6;
	static initonly DWORD DefaultDdsMipLevels = 7;

private:
	static array<BYTE> ^GetTextureFrom32bppArgb(System::IntPtr bitmapData, UINT32 width, UINT32 height, DWORD stride, MpvrEncodePreset &preset);
	static array<BYTE> ^GetTextureFromImage(System::Drawing::Bitmap ^bitmap, MpvrEncodePreset &preset);

};

DECL_NS_END_MPVR()

MPVR_EXPORTS(VOID)  MpvrCompressPvrTextureFrom32bppArgb(PVOID pBitmapData, DWORD dwWidth, DWORD dwHeight, DWORD dwStride, DWORD dwMipLevels, OUT PVOID *pCompressedTexture, OUT PDWORD pdwTextureDataSize);
MPVR_EXPORTS(PBYTE) MpvrCompressPvrTexture(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, DWORD dwStride, UINT32 dwMipLevels, PixelType eInputPixelType, BOOL bIsPremultiplied, OUT PDWORD *ppdwDataSizes);
