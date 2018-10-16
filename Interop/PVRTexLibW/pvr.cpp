#include "pvr.h"

#include <Windows.h>

#include "external/PVRTexTool/Include/PVRTexture.h"
#include "external/PVRTexTool/Include/PVRTextureUtilities.h"

using namespace pvrtexture;

struct MpvrEncodePreset {

    PixelType pixelType;
    ECompressorQuality quality;
    LPCSTR extension;
    DWORD mipLevels;

};

static const int DEFAULT_PVR_MIP_LEVELS = 7;
#define PIXEL_FORMAT_GDIPLUS_LIB_USED (PVRTGENPIXELID4('b', 'g', 'r', 'a', 8, 8, 8, 8))

static MpvrEncodePreset g_pvrEncPreset{
    PixelType(ePVRTPF_ETC2_RGB),
    eETCFast,
    ".pvr",
    DEFAULT_PVR_MIP_LEVELS
};

static bool_t MpvrCompressTextureInternal(void *pData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, PixelType eInputPixelType, bool_t bIsPremultiplied,
    uint32_t dwOutputMipLevels, PixelType eOutputPixelType, ECompressorQuality eQuality, CPVRTexture **ppTexture);

bool_t STDCALL MpvrCompressPvrTextureFrom32bppArgb(void *pBitmapData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, uint32_t dwMipLevels, uint8_t **ppCompressedTextureData, uint32_t *pdwTextureDataSize) {
    uint32_t *textureSizes;

    const auto pixelFormat = PIXEL_FORMAT_GDIPLUS_LIB_USED;
    const auto textureData = MpvrCompressPvrTexture(pBitmapData, dwWidth, dwHeight, dwStride, dwMipLevels, pixelFormat, FALSE, &textureSizes);

    const bool_t successful = ppCompressedTextureData ? TRUE : FALSE;

    if (!ppCompressedTextureData) {
        delete textureData;
    } else {
        *ppCompressedTextureData = textureData;
    }

    if (pdwTextureDataSize) {
        *pdwTextureDataSize = textureSizes[0];
    }

    delete[] textureSizes;

    return successful;
}

// Basic idea: https://github.com/SickheadGames/ManagedPVRTC/blob/master/PVRTexLibWrapper/PVRTexLibWrapper.cpp
/// <summary>
/// Core function to compress PowerVR textures.
/// </summary>
/// <param name="pData">Bitmap data (32bpp RGBA).</param>
/// <param name="dwMipLevels">Expected mipmap level. If no mipmapping is needed, pass it 0.</param>
uint8_t *STDCALL MpvrCompressPvrTexture(void *pData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, uint32_t dwMipLevels, PixelType eInputPixelType, bool_t bIsPremultiplied, uint32_t **ppdwDataSizes) {
    CPVRTexture *texture;

    MpvrCompressTextureInternal(pData, dwWidth, dwHeight, dwStride, eInputPixelType, bIsPremultiplied, dwMipLevels, g_pvrEncPreset.pixelType, g_pvrEncPreset.quality, &texture);

    if (!texture) {
        return nullptr;
    }

    if (ppdwDataSizes) {
        const auto pdwDataSizes = new uint32_t[dwMipLevels];

        *ppdwDataSizes = pdwDataSizes;

        for (uint32_t x = 0; x < dwMipLevels; x++) {
            pdwDataSizes[x] = texture->getDataSize(x);
        }
    }

    const auto totalDataSize = texture->getDataSize();
    const auto returnData = new BYTE[totalDataSize];

    memcpy(returnData, texture->getDataPtr(), totalDataSize);

    delete texture;

    return returnData;
}

void __stdcall MpvrFreeTexture(void *pTextureData) {
    const auto p = static_cast<uint8_t *>(pTextureData);

    delete[] p;
}

bool_t MpvrCompressTextureInternal(void *pData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, PixelType eInputPixelType, bool_t bIsPremultiplied,
    uint32_t dwOutputMipLevels, PixelType eOutputPixelType, ECompressorQuality eQuality, CPVRTexture **ppTexture) {
    if (!ppTexture) {
        return FALSE;
    }

    PVRTextureHeaderV3 pvrHeader;
    pvrHeader.u32Version = PVRTEX_CURR_IDENT;
    pvrHeader.u32Flags = bIsPremultiplied ? PVRTEX3_PREMULTIPLIED : 0;
    pvrHeader.u64PixelFormat = eInputPixelType.PixelTypeID;
    pvrHeader.u32ColourSpace = ePVRTCSpacelRGB;
    pvrHeader.u32ChannelType = ePVRTVarTypeUnsignedByteNorm;
    pvrHeader.u32Width = dwWidth;
    pvrHeader.u32Height = dwHeight;
    pvrHeader.u32Depth = 1;
    pvrHeader.u32NumSurfaces = 1;
    pvrHeader.u32NumFaces = 1;
    pvrHeader.u32MIPMapCount = dwOutputMipLevels;
    pvrHeader.u32MetaDataSize = 0;

    PBYTE p = nullptr;
    // TODO: We choose the format ARGB8888, so stride always equals to width. Ignore this parameter.
    p = static_cast<PBYTE>(pData);

    const auto pTexture = new CPVRTexture(pvrHeader);

    *ppTexture = pTexture;

    memcpy(pTexture->getDataPtr(), p, dwStride * dwHeight);

    if (p != pData) {
        delete[] p;
    }

    if (dwOutputMipLevels > 1) {
        GenerateMIPMaps(*pTexture, eResizeLinear, dwOutputMipLevels);
    }

    Transcode(*pTexture, eOutputPixelType, ePVRTVarTypeUnsignedByteNorm, ePVRTCSpacelRGB, eQuality);

    return TRUE;
}
