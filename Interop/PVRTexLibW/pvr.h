#pragma once

#include "common.h"

#include <cstdint>

namespace pvrtexture {

    union PixelType;

}

typedef uint32_t bool_t;

MPVR_EXPORTS(uint8_t *) MpvrCompressPvrTexture(void *pData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, uint32_t dwMipLevels, pvrtexture::PixelType eInputPixelType, bool_t bIsPremultiplied, uint32_t **ppdwDataSizes);
MPVR_EXPORTS(bool_t) MpvrCompressPvrTextureFrom32bppArgb(void *pBitmapData, uint32_t dwWidth, uint32_t dwHeight, uint32_t dwStride, uint32_t dwMipLevels, uint8_t **ppCompressedTextureData, uint32_t *pdwTextureDataSize);
MPVR_EXPORTS(void) MpvrFreeTexture(void *pTextureData);
