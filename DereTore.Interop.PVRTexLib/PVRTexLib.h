#pragma once
#include "mpvr_header.h"
#include <Windows.h>

DECL_NS_BEGIN_MPVR()

public ref class PvrUtilities abstract sealed {

public:
	static DWORD GetTextureDataSizeFrom32bppArgb(System::IntPtr bitmapData, UINT32 width, UINT32 height);
	static DWORD GetTextureDataSizeFrom32bppArgb(System::IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels);
	static array<BYTE> ^GetFullTextureFromImage(System::Drawing::Bitmap ^bitmap);
	static array<BYTE> ^GetFullTextureFromImage(System::Drawing::Bitmap ^bitmap, DWORD mipLevels);

private:
	static array<BYTE> ^CompressTextureFrom32bppArgb(System::IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels);
	static array<BYTE> ^GetFullCompressedTextureFrom32bppArgb(System::IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels);

	static DWORD DefaultMipLevels = 6;

};

DECL_NS_END_MPVR()

MPVR_EXPORTS(VOID)  MpvrCompressTextureFrom32bppArgb(PVOID pBitmapData, DWORD dwWidth, DWORD dwHeight, DWORD dwMipLevels, OUT PVOID *pCompressedTexture, OUT PDWORD pdwTextureDataSize);
MPVR_EXPORTS(PBYTE) MpvrCompressTextureEx(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, UINT32 dwMipLevels, UINT64 ullPixelFormat, BOOL bIsPremultiplied, OUT PDWORD *ppdwDataSizes);
