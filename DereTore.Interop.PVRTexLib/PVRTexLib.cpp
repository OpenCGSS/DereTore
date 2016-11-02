#define _WINDLL_IMPORT // for CPVRTString
#include "PVRTexture.h"
#include "PVRTextureUtilities.h"
#include "PVRTexLib.h"

using namespace System;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;

#define PIXEL_FORMAT_GDIPLUS_LIB_USED (PVRTGENPIXELID4('b', 'g', 'r', 'a', 8, 8, 8, 8))

VOID STDCALL MpvrCompressTextureFrom32bppArgb(PVOID pBitmapData, DWORD dwWidth, DWORD dwHeight, DWORD dwMipLevels, OUT PVOID *pCompressedTexture, OUT PDWORD pdwTextureDataSize) {
	PDWORD textureSizes;
	auto pixelFormat = PIXEL_FORMAT_GDIPLUS_LIB_USED;
	auto textureData = MpvrCompressTextureEx(pBitmapData, dwWidth, dwHeight, dwMipLevels, pixelFormat, FALSE, OUT &textureSizes);
	if (!pCompressedTexture) {
		delete textureData;
	} else {
		*pCompressedTexture = textureData;
	}
	if (pdwTextureDataSize) {
		*pdwTextureDataSize = textureSizes[0];
	}
	delete textureSizes;
}

void MpvrCompressTextureInternal(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, UINT32 dwMipLevels, UINT64 ullPixelFormat, BOOL bIsPremultiplied, OUT pvrtexture::CPVRTexture **ppTexture) {
	PVRTextureHeaderV3 pvrHeader;
	pvrHeader.u32Version = PVRTEX_CURR_IDENT;
	pvrHeader.u32Flags = bIsPremultiplied ? PVRTEX3_PREMULTIPLIED : 0;
	pvrHeader.u64PixelFormat = ullPixelFormat;
	pvrHeader.u32ColourSpace = EPVRTColourSpace::ePVRTCSpacelRGB;
	pvrHeader.u32ChannelType = EPVRTVariableType::ePVRTVarTypeUnsignedByteNorm;
	pvrHeader.u32Width = dwWidth;
	pvrHeader.u32Height = dwHeight;
	pvrHeader.u32Depth = 1;
	pvrHeader.u32NumSurfaces = 1;
	pvrHeader.u32NumFaces = 1;
	pvrHeader.u32MIPMapCount = dwMipLevels + 1;
	pvrHeader.u32MetaDataSize = 0;

	*ppTexture = new pvrtexture::CPVRTexture(pvrHeader, pData);
	if (dwMipLevels > 0) {
		pvrtexture::GenerateMIPMaps(**ppTexture, pvrtexture::EResizeMode::eResizeLinear, dwMipLevels + 1);
	}
	Debug::Print("Exp. mip levels: " + static_cast<Int32>(dwMipLevels).ToString());

	// Unity uses ETC2_RGB in its textures ~128x128
	pvrtexture::PixelType pixelType(ePVRTPF_ETC2_RGB);
	pvrtexture::Transcode(**ppTexture, pixelType, EPVRTVariableType::ePVRTVarTypeUnsignedByteNorm, EPVRTColourSpace::ePVRTCSpacelRGB, pvrtexture::ECompressorQuality::eETCFast);
}

// Basic idea: https://github.com/SickheadGames/ManagedPVRTC/blob/master/PVRTexLibWrapper/PVRTexLibWrapper.cpp
/// <summary>
/// Core function to compress PowerVR textures.
/// <param name="dwMipLevels">Expected mipmap level. If no mipmapping is needed, pass it 0.</param>
/// </summary>
PBYTE STDCALL MpvrCompressTextureEx(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, UINT32 dwMipLevels, UINT64 ullPixelFormat, BOOL bIsPremultiplied, OUT PDWORD *ppdwDataSizes) {
	pvrtexture::CPVRTexture *texture;
	MpvrCompressTextureInternal(pData, dwWidth, dwHeight, dwMipLevels, ullPixelFormat, bIsPremultiplied, OUT &texture);

	*ppdwDataSizes = new DWORD[dwMipLevels + 1];
	for (auto x = 0; x <= dwMipLevels; x++) {
		(*ppdwDataSizes)[x] = texture->getDataSize(x);
	}

	auto totalDataSize = texture->getDataSize();
	auto returnData = new BYTE[totalDataSize];
	memcpy(returnData, texture->getDataPtr(), totalDataSize);
	delete texture;
	return returnData;
}

BOOL MpvrCompressTextureToFile(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, UINT32 dwMipLevels, UINT64 ullPixelFormat, BOOL bIsPremultiplied, LPCSTR lpstrFileName) {
	pvrtexture::CPVRTexture *texture;
	MpvrCompressTextureInternal(pData, dwWidth, dwHeight, dwMipLevels, ullPixelFormat, bIsPremultiplied, OUT &texture);
	CPVRTString fileName(lpstrFileName);
	auto succeeded = texture->saveFile(fileName);
	delete texture;
	return succeeded;
}

DECL_NS_BEGIN_MPVR()

array<BYTE> ^PvrUtilities::CompressTextureFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels) {
	DWORD dataSize;
	PVOID texture;
	MpvrCompressTextureFrom32bppArgb(static_cast<PVOID>(bitmapData), width, height, mipLevels, OUT &texture, OUT &dataSize);
	auto compressedTexture = gcnew array<BYTE>(dataSize);
	Marshal::Copy(IntPtr(texture), compressedTexture, 0, dataSize);
	delete texture;
	return compressedTexture;
}

DWORD PvrUtilities::GetTextureDataSizeFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels) {
	DWORD dataSize;
	MpvrCompressTextureFrom32bppArgb(static_cast<PVOID>(bitmapData), width, height, mipLevels, nullptr, OUT &dataSize);
	return dataSize;
}

DWORD PvrUtilities::GetTextureDataSizeFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height) {
	return GetTextureDataSizeFrom32bppArgb(bitmapData, width, height, DefaultMipLevels);
}

array<BYTE> ^PvrUtilities::GetFullCompressedTextureFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height, DWORD mipLevels) {
	array<BYTE> ^textureFileData = nullptr;
	CHAR tempPath[MAX_PATH] = { 0 }, tempFileName[MAX_PATH] = { 0 };
	GetTempPath(MAX_PATH, tempPath);
	GetTempFileName(tempPath, "pvr", 0, tempFileName);
	for (auto i = strlen(tempFileName) - 1; i > 0; --i) {
		if (tempFileName[i] == '.') {
			tempFileName[i] = '\0';
			break;
		} else if (tempFileName[i] == '\\' || tempFileName[i] == '/') {
			break;
		}
	}
	strcat(tempFileName, ".pvr");
	PDWORD textureSizes;
	auto originalPvrPixelFormat = PIXEL_FORMAT_GDIPLUS_LIB_USED;
	auto textureData = MpvrCompressTextureToFile(static_cast<PVOID>(bitmapData), width, height, mipLevels, originalPvrPixelFormat, FALSE, tempFileName);
	// TODO: Any other solutions?
	auto hFile = CreateFile(tempFileName, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr);
	if (hFile && hFile != INVALID_HANDLE_VALUE) {
		Debug::Print("Opening temp file " + gcnew String(tempFileName));
		DWORD fileSize = GetFileSize(hFile, nullptr);
		auto buffer = new BYTE[fileSize];
		DWORD sizeRead;
		ReadFile(hFile, buffer, fileSize, &sizeRead, nullptr);
		textureFileData = gcnew array<BYTE>(fileSize);
		Marshal::Copy(IntPtr(buffer), textureFileData, 0, fileSize);
		delete buffer;
		CloseHandle(hFile);
	} else {
		Debug::Print("ERROR: Unable to open temp file " + gcnew String(tempFileName));
	}
	DeleteFile(tempFileName);
	return textureFileData;
}

array<BYTE> ^PvrUtilities::GetFullTextureFromImage(Bitmap ^bitmap) {
	return GetFullTextureFromImage(bitmap, DefaultMipLevels);
}

array<BYTE> ^PvrUtilities::GetFullTextureFromImage(Bitmap ^bitmap, DWORD mipLevels) {
	auto actualBitmap = dynamic_cast<Bitmap ^>(bitmap->Clone());
	// Don't have any idea. Unity just does it this way.
	actualBitmap->RotateFlip(RotateFlipType::RotateNoneFlipY);
	auto bitmapData = actualBitmap->LockBits(System::Drawing::Rectangle(0, 0, actualBitmap->Width, actualBitmap->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb);
	auto textureData = GetFullCompressedTextureFrom32bppArgb(bitmapData->Scan0, bitmapData->Width, bitmapData->Height, mipLevels);
	actualBitmap->UnlockBits(bitmapData);
	delete actualBitmap;
	return textureData;
}

DECL_NS_END_MPVR()
