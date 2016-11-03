#include "d3dx9_utils.h"

using namespace DereTore::Interop::D3DX9;
using namespace System::Drawing::Imaging;

struct D3D_STAT {

	HWND hFocusWindow;
	LPDIRECT3D9 pDirect3D;
	IDirect3DDevice9 *pDevice;
	LPDIRECT3DTEXTURE9 pTexture;
	ID3DXBuffer *pBuffer;

	void Cleanup() {
		if (pBuffer) {
			pBuffer->Release();
		}
		if (pTexture) {
			pTexture->Release();
		}
		if (pDevice) {
			pDevice->Release();
		}
		if (pDirect3D) {
			pDirect3D->Release();
		}
		if (hFocusWindow) {
			DestroyWindow(hFocusWindow);
		}
	}
};

#define ARGB888_RED_MASK      0x00ff0000
#define ARGB888_GREEN_MASK    0x0000ff00
#define ARGB888_BLUE_MASK     0x000000ff

PBYTE Argb8888ToRgb565(PVOID data, UINT32 width, UINT32 height, UINT32 stride, OUT PDWORD newSize) {
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
	auto newStride = width * sizeof(RGB565);
	if (newStride % 4 != 0) {
		newStride += 4 - (newStride % 4);
	}
	auto pRgb565 = new BYTE[newStride * height];
	if (newSize) {
		*newSize = newStride * height;
	}
	ZeroMemory(pRgb565, newStride * height);
	for (auto j = 0; j < height; ++j) {
		auto originalRowStart = static_cast<PBYTE>(data) + stride * j;
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
	return pRgb565;
}

array<BYTE> ^D3dx9Utilities::GetDdsTextureFromImage(Bitmap ^bitmap) {
	auto actualBitmap = dynamic_cast<Bitmap ^>(bitmap->Clone());
	actualBitmap->RotateFlip(RotateFlipType::RotateNoneFlipY);
	auto bitmapData = actualBitmap->LockBits(System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height), ImageLockMode::ReadOnly, PixelFormat::Format32bppArgb);
	array<BYTE> ^data;
	auto hr = GetDdsTextureFromPtr(bitmapData->Scan0, bitmapData->Width, bitmapData->Height, bitmapData->Stride, data);
	actualBitmap->UnlockBits(bitmapData);
	delete actualBitmap;
	return data;
}

HRESULT D3dx9Utilities::GetDdsTextureFromPtr(IntPtr data, UINT32 width, UINT32 height, UINT32 stride, array<BYTE> ^%ddsFileData) {
	return GetDdsTextureFromPtr(static_cast<PVOID>(data), width, height, stride, ddsFileData);
}

#define QUICK_FAIL() 	if (FAILED(hr)) { \
							stat.Cleanup(); \
							ddsFileData = nullptr; \
							return hr; \
						}

HRESULT D3dx9Utilities::GetDdsTextureFromPtr(PVOID data, UINT32 width, UINT32 height, UINT32 stride, array<BYTE> ^%ddsFileData) {
	D3D_STAT stat = { 0 };

	stat.pDirect3D = Direct3DCreate9(D3D_SDK_VERSION);
	if (!stat.pDirect3D) {
		return E_FAIL;
	}

	HRESULT hr;

	stat.hFocusWindow = CreateWindow(L"STATIC", L"dummy", 0, 0, 0, 100, 100, nullptr, nullptr, nullptr, nullptr);

	D3DPRESENT_PARAMETERS presentParams;
	memset(&presentParams, 0, sizeof(presentParams));
	presentParams.BackBufferWidth = presentParams.BackBufferHeight = 100;
	presentParams.BackBufferCount = 1;
	presentParams.BackBufferFormat = D3DFMT_A8R8G8B8;
	presentParams.SwapEffect = D3DSWAPEFFECT_DISCARD;
	presentParams.Windowed = TRUE;
	presentParams.PresentationInterval = D3DPRESENT_INTERVAL_DEFAULT;
	hr = stat.pDirect3D->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, stat.hFocusWindow, D3DCREATE_MIXED_VERTEXPROCESSING, &presentParams, &stat.pDevice);

	QUICK_FAIL();

	hr = D3DXCreateTexture(stat.pDevice, width, height, DefaultDdsMipLevels, 0, D3DFMT_R5G6B5, D3DPOOL::D3DPOOL_SCRATCH, &stat.pTexture);
	QUICK_FAIL();

	D3DLOCKED_RECT lockedRect;
	hr = stat.pTexture->LockRect(0, &lockedRect, nullptr, 0);
	QUICK_FAIL();
	DWORD rgbArraySize;
	auto newData = Argb8888ToRgb565(data, width, height, stride, &rgbArraySize);
	memcpy(lockedRect.pBits, newData, rgbArraySize);
	delete[] newData;
	hr = stat.pTexture->UnlockRect(0);
	QUICK_FAIL();

	stat.pTexture->GenerateMipSubLevels();

	hr = D3DXSaveTextureToFileInMemory(&stat.pBuffer, D3DXIMAGE_FILEFORMAT::D3DXIFF_DDS, stat.pTexture, nullptr);
	QUICK_FAIL();

	auto pData = stat.pBuffer->GetBufferPointer();
	auto dataSize = stat.pBuffer->GetBufferSize();
	ddsFileData = gcnew array<BYTE>(dataSize);
	Marshal::Copy(IntPtr(pData), ddsFileData, 0, dataSize);

	stat.Cleanup();

	return S_OK;
}
