#define _WINDLL_IMPORT // for CPVRTString
#include "PVRTexture.h"
#include "PVRTextureUtilities.h"
#include "PVRTexLib.h"

using namespace System;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace DereTore::Interop::PVRTexLib;

// TODO: The DDS preset doesn't seem to work...
MpvrEncodePreset DdsEncPreset{ PVRTGENPIXELID3('b', 'g', 'r', 5, 6, 5), ECompressorQuality::ePVRTCNormal, ".dds", PvrUtilities::DefaultDdsMipLevels };
MpvrEncodePreset PvrEncPreset{ PixelType(ePVRTPF_ETC2_RGB), ECompressorQuality::eETCFast, ".pvr", PvrUtilities::DefaultPvrMipLevels };

#define PIXEL_FORMAT_GDIPLUS_LIB_USED (PVRTGENPIXELID4('b', 'g', 'r', 'a', 8, 8, 8, 8))

VOID STDCALL MpvrCompressPvrTextureFrom32bppArgb(PVOID pBitmapData, DWORD dwWidth, DWORD dwHeight, DWORD dwStride, DWORD dwMipLevels, OUT PVOID *pCompressedTexture, OUT PDWORD pdwTextureDataSize) {
	PDWORD textureSizes;
	auto pixelFormat = PIXEL_FORMAT_GDIPLUS_LIB_USED;
	auto textureData = MpvrCompressPvrTexture(pBitmapData, dwWidth, dwHeight, dwStride, dwMipLevels, pixelFormat, FALSE, OUT &textureSizes);
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

void MpvrCompressTextureInternal(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, DWORD dwStride, PixelType eInputPixelType, BOOL bIsPremultiplied,
	UINT32 dwOutputMipLevels, PixelType eOutputPixelType, ECompressorQuality eQuality, OUT CPVRTexture &ppTexture) {
	PVRTextureHeaderV3 pvrHeader;
	pvrHeader.u32Version = PVRTEX_CURR_IDENT;
	pvrHeader.u32Flags = bIsPremultiplied ? PVRTEX3_PREMULTIPLIED : 0;
	pvrHeader.u64PixelFormat = eInputPixelType.PixelTypeID;
	pvrHeader.u32ColourSpace = EPVRTColourSpace::ePVRTCSpacelRGB;
	pvrHeader.u32ChannelType = EPVRTVariableType::ePVRTVarTypeUnsignedByteNorm;
	pvrHeader.u32Width = dwWidth;
	pvrHeader.u32Height = dwHeight;
	pvrHeader.u32Depth = 1;
	pvrHeader.u32NumSurfaces = 1;
	pvrHeader.u32NumFaces = 1;
	pvrHeader.u32MIPMapCount = dwOutputMipLevels;
	pvrHeader.u32MetaDataSize = 0;

	PBYTE p = nullptr;
	// TODO: We choose the format ARGB8888, so stride always equals to width. Ignore this paramater.
	p = static_cast<PBYTE>(pData);

	ppTexture = CPVRTexture(pvrHeader);
	memcpy(ppTexture.getDataPtr(), p, dwStride * dwHeight);
	if (p != pData) {
		delete[] p;
	}
	if (dwOutputMipLevels > 1) {
		GenerateMIPMaps(ppTexture, EResizeMode::eResizeLinear, dwOutputMipLevels);
	}

	Transcode(ppTexture, eOutputPixelType, EPVRTVariableType::ePVRTVarTypeUnsignedByteNorm, EPVRTColourSpace::ePVRTCSpacelRGB, eQuality);
}

// Basic idea: https://github.com/SickheadGames/ManagedPVRTC/blob/master/PVRTexLibWrapper/PVRTexLibWrapper.cpp
/// <summary>
/// Core function to compress PowerVR textures.
/// </summary>
/// <param name="dwMipLevels">Expected mipmap level. If no mipmapping is needed, pass it 0.</param>
PBYTE STDCALL MpvrCompressPvrTexture(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, DWORD dwStride, UINT32 dwMipLevels, PixelType eInputPixelType, BOOL bIsPremultiplied, OUT PDWORD *ppdwDataSizes) {
	CPVRTexture texture;
	MpvrCompressTextureInternal(pData, dwWidth, dwHeight, dwStride, eInputPixelType, bIsPremultiplied, dwMipLevels, PvrEncPreset.pixelType, PvrEncPreset.quality, OUT texture);

	*ppdwDataSizes = new DWORD[dwMipLevels];
	for (auto x = 0; x < dwMipLevels; x++) {
		(*ppdwDataSizes)[x] = texture.getDataSize(x);
	}

	auto totalDataSize = texture.getDataSize();
	auto returnData = new BYTE[totalDataSize];
	memcpy(returnData, texture.getDataPtr(), totalDataSize);
	return returnData;
}

BOOL MpvrCompressTextureToFile(PVOID pData, UINT32 dwWidth, UINT32 dwHeight, DWORD dwStride, PixelType eInputPixelType, BOOL bIsPremultiplied, MpvrEncodePreset &preset, LPCSTR lpstrFileName) {
	CPVRTexture texture;
	MpvrCompressTextureInternal(pData, dwWidth, dwHeight, dwStride, eInputPixelType, bIsPremultiplied, preset.mipLevels, preset.pixelType, preset.quality, OUT texture);
	CPVRTString fileName(lpstrFileName);
	auto succeeded = texture.saveFile(fileName);
	Debug::Print("Saving to " + gcnew String(lpstrFileName) + " successful: " + succeeded);
	return succeeded;
}

DECL_NS_BEGIN_MPVR()

array<BYTE> ^PvrUtilities::GetTextureFrom32bppArgb(IntPtr bitmapData, UINT32 width, UINT32 height, DWORD stride, MpvrEncodePreset &preset) {
	array<BYTE> ^textureFileData = nullptr;
	CHAR tempPath[MAX_PATH] = { 0 }, tempFileName[MAX_PATH] = { 0 };
	GetTempPath(MAX_PATH, tempPath);
	GetTempFileName(tempPath, "tex", 0, tempFileName);
	for (auto i = strlen(tempFileName) - 1; i > 0; --i) {
		if (tempFileName[i] == '.') {
			tempFileName[i] = '\0';
			break;
		} else if (tempFileName[i] == '\\' || tempFileName[i] == '/') {
			break;
		}
	}
	strcat(tempFileName, preset.extension);
	PDWORD textureSizes;
	auto originalPvrPixelFormat = PIXEL_FORMAT_GDIPLUS_LIB_USED;
	auto textureData = MpvrCompressTextureToFile(static_cast<PVOID>(bitmapData), width, height, stride, originalPvrPixelFormat, FALSE, preset, tempFileName);
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

array<BYTE> ^PvrUtilities::GetTextureFromImage(Bitmap ^bitmap, MpvrEncodePreset &preset) {
	auto actualBitmap = dynamic_cast<Bitmap ^>(bitmap->Clone());
	// Don't have any idea. Unity just does it this way.
	actualBitmap->RotateFlip(RotateFlipType::RotateNoneFlipY);
	auto bitmapData = actualBitmap->LockBits(System::Drawing::Rectangle(0, 0, actualBitmap->Width, actualBitmap->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb);
	auto textureData = GetTextureFrom32bppArgb(bitmapData->Scan0, bitmapData->Width, bitmapData->Height, bitmapData->Stride, preset);
	actualBitmap->UnlockBits(bitmapData);
	delete actualBitmap;
	return textureData;
}

array<BYTE> ^PvrUtilities::GetPvrTextureFromImage(Bitmap ^bitmap) {
	return GetTextureFromImage(bitmap, PvrEncPreset);
}
array<BYTE> ^PvrUtilities::GetDdsTextureFromImage(Bitmap ^bitmap) {
	return GetTextureFromImage(bitmap, DdsEncPreset);
}

DECL_NS_END_MPVR()
