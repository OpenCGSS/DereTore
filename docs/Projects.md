# Projects

## Core Libraries

[**StarlightStage**](../Common/DereTore.Common.StarlightStage)

You know what it is.

[**HCA Audio**](../Exchange/DereTore.Exchange.Audio.HCA)

The pure C# implementation of CRI HCA v2.0 decoder, based on [kawashima](https://github.com/hozuki/kawashima).
Here is its [readme](../Exchange/DereTore.Exchange.Audio.HCA/README.md).

[**ACB Archive**](../Exchange/DereTore.Exchange.Archive.ACB)

CRI ACB/AWB package manipulation library. Only necessary parts are implemented. Here is its [readme](../Exchange/DereTore.Exchange.Archive.ACB/README.md).

## Applications

Applications can be found in [`Apps`](Apps).

**Starlight Director**

Removed for platform compatibility. The new version can be found at [hozuki/StarlightDirector](https://github.com/hozuki/StarlightDirector).

[**Score Viewer**](../Apps/ScoreViewer)

Viewing scores (beatmaps) of live music. Here is its [readme](../Apps/ScoreViewer/README.md).

This utility is somewhat unpolished, but it shows the basic ideas about CGSS beatmaps and how to read and display them.

[**hcacc**](../Apps/Hcacc)

The C# version of HCA cipher conversion utility. See **hcacc** in [libcgss](https://github.com/hozuki/libcgss).

[**hcaenc**](../Apps/Hcaenc)

The C# version of HCA encoding utility. See **hcaenc** in [libcgss](https://github.com/hozuki/libcgss).

[**hca2wav**](../Apps/Hca2Wav)

A console appilcation that converts HCA audio to wave audio. It can be used as a demo to integrate
*DereTore.HCA*.

[**ACB Maker**](../Apps/AcbMaker)

A tool for creating CGSS-compatible ACB archives. Here is its [readme](../Apps/AcbMaker/README.md).

[**Music Toolchain**](Apps/MusicToolchain)

A GUI program that integrates **hcaenc**, **hcacc** and **ACB Maker** into a streamline. Here
is its [readme](../Apps/MusicToolchain/README.md).

[**ACB Unzip**](../Apps/AcbUnzip)

An application for unpacking ACB archives.

[**Jacket Creator**](../Apps/JacketCreator)

A utility for creating CD jackets. Used with score and music makers.

[**acb2wavs**](../Apps/Acb2Wavs)

A console application that converts all HCAs in an ACB to wave audio.

## Other

**LZ4**

An LZ4 compression utility for compressing game data. It is designed for IdolProxy.

**PVRTexLib Interop**

An interop library of [PVRTexLib](https://community.imgtec.com/developers/powervr/graphics-sdk/), for generating PVR textures.

**D3DX9 Interop**

An interop library using D3DX9 to generate DDS textures.

**Unity Engine Asset IO**

An library which provides functions to generate CGSS-compatible (for specific Unity version) asset bundles.
