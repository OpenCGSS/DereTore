#include "dds_utils.h"

using namespace System::Diagnostics;
using namespace System::Runtime::InteropServices;

using namespace DereTore::Interop::DirectXTex;

array<BYTE> ^DdsUtilities::GetDdsTextureFromImage(Bitmap ^bitmap) {
	return GetDdsTextureFromImage(bitmap, false);
}

array<BYTE> ^DdsUtilities::GetDdsTextureFromImage(Bitmap ^bitmap, bool withHeader) {
	auto actualBitmap = dynamic_cast<Bitmap ^>(bitmap->Clone());
	actualBitmap->RotateFlip(RotateFlipType::RotateNoneFlipY);
	auto bitmapData = actualBitmap->LockBits(System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb);
	auto data = GetDdsTextureFromPtr(bitmapData->Scan0, bitmapData->Width, bitmapData->Height, bitmapData->Stride, withHeader);
	actualBitmap->UnlockBits(bitmapData);
	delete actualBitmap;
	return data;
}

struct RGB565 {
	BYTE r : 5;
	BYTE gh : 3;
	BYTE gl : 3;
	BYTE b : 5;
};

struct ARGB8888 {
	BYTE b;
	BYTE g;
	BYTE r;
	BYTE a;
};

#define ARGB888_RED_MASK      0x00ff0000
#define ARGB888_GREEN_MASK    0x0000ff00
#define ARGB888_BLUE_MASK     0x000000ff

// Source format: ARGB8888
array<BYTE> ^DdsUtilities::GetDdsTextureFromPtr(IntPtr data, DWORD width, DWORD height, DWORD stride, bool withHeader) {
	auto newStride = width * sizeof(RGB565);
	if (newStride % 4 != 0) {
		newStride += 4 - (newStride % 4);
	}
	DirectX::Image image;
	image.width = width;
	image.height = height;
	image.format = DXGI_FORMAT_B5G6R5_UNORM;
	TexMetadata metadata = {};
	metadata.width = image.width;
	metadata.height = image.height;
	metadata.depth = 1;
	metadata.arraySize = 1;
	metadata.mipLevels = DefaultDdsMipLevels;
	metadata.format = image.format;
	metadata.dimension = TEX_DIMENSION::TEX_DIMENSION_TEXTURE2D;
	image.rowPitch = newStride;
	image.slicePitch = width * height * sizeof(RGB565);

	// ARGB8888 -> RGB565
	// Yes we can use the Convert() function in DirectXTex, but what's the fun there?
	auto pRgb565 = new BYTE[newStride * height];
	memset(pRgb565, 0, newStride * height);
	for (auto j = 0; j < height; ++j) {
		auto originalRowStart = static_cast<PBYTE>(static_cast<PVOID>(data)) + stride * j;
		auto newRowStart = pRgb565 + newStride * j;
		for (auto i = 0; i < width; ++i) {
			auto argb = (DWORD *)(originalRowStart + sizeof(DWORD) * i);
			BYTE r = (*argb & ARGB888_RED_MASK) >> 19;
			BYTE g = (*argb & ARGB888_GREEN_MASK) >> 10;
			BYTE b = (*argb & ARGB888_BLUE_MASK) >> 3;
			auto rgb = (WORD *)(newRowStart + sizeof(WORD) * i);
			*rgb = (r << 11) | (g << 5) | b;
		}
	}
	image.pixels = pRgb565;
	ScratchImage scratchImage;
	HRESULT hr = GenerateMipMaps(image, TEX_FILTER_FLAGS::TEX_FILTER_DEFAULT, 0, scratchImage);

	if (withHeader) {
		WCHAR tempPath[MAX_PATH] = { 0 }, tempFileName[MAX_PATH] = { 0 };
		GetTempPath(MAX_PATH, tempPath);
		GetTempFileName(tempPath, L"dds", 0, tempFileName);
		wcscat_s(tempFileName, L".dds");

		auto images = scratchImage.GetImages();
		auto imageCount = scratchImage.GetImageCount();
		hr = SaveToDDSFile(images, imageCount, metadata, DDS_FLAGS::DDS_FLAGS_NONE, tempFileName);
		delete[] pRgb565;
		if (FAILED(hr)) {
			Debug::Print(L"An error occurred: HResult=" + (static_cast<Int32>(hr)).ToString(L"x8"));
			return nullptr;
		}

		auto tempFileNameM = gcnew String(tempFileName);
		array<BYTE> ^fileData = nullptr;
		if (SUCCEEDED(hr)) {
			fileData = File::ReadAllBytes(tempFileNameM);
			Debug::Print(L"Opening file " + tempFileNameM);
		} else {
			Debug::Print(L"An error occurred: HResult=" + (static_cast<Int32>(hr)).ToString(L"x8"));
		}
		File::Delete(tempFileNameM);
		return fileData;
	} else {
		auto pixels = scratchImage.GetPixels();
		auto size = scratchImage.GetPixelsSize();
		auto managedData = gcnew array<BYTE>(size);
		Marshal::Copy(IntPtr(pixels), managedData, 0, size);
		return managedData;
	}
}
