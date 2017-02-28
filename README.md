# DereTore

[![AppVeyor](https://img.shields.io/appveyor/ci/hozuki/deretore-avoh8.svg)](https://ci.appveyor.com/project/hozuki/deretore-avoh8)
[![GitHub contributors](https://img.shields.io/github/contributors/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore/graphs/contributors)
[![Libraries.io for GitHub](https://img.shields.io/librariesio/github/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore)
[![Github All Releases](https://img.shields.io/github/downloads/OpenCGSS/DereTore/total.svg)](https://github.com/OpenCGSS/DereTore/releases)

The goal of DereTore is to improve gaming experience in [Idolmaster Cinderella Girls: Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage)
(CGSS/DereSute), or even to customize it a little bit.

**Downloads:**

- [Nightly build](https://ci.appveyor.com/api/projects/hozuki/deretore-avoh8/artifacts/deretore-toolkit-x86.zip?job=Platform%3A+x86) (Windows, x86)
- [Releases](https://github.com/OpenCGSS/DereTore/releases)

Wonder [how this name comes from](Notes.md#the-name)?

## Building

1. Clone from [GitHub](https://github.com/OpenCGSS/DereTore.git): `git clone https://github.com/OpenCGSS/DereTore.git`;
2. Install missing NuGet packages: `nuget restore DereTore.sln` (or use NuGet Package Manager in Visual Studio);
3. Open `DereTore.sln` in Visual Studio (VS2010 SP1 or later is required);
4. Build the solution.

## Usage

Requirements:

- Windows 7 or later (though some tools should be able to run on Mono)
- [.NET Framework 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=17718)
- [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784) <sup>(*)</sup>
- [DirectX 9.0c](https://www.microsoft.com/en-us/download/details.aspx?id=8109) <sup>(*)</sup>

_<sup>*</sup> Only needed when you want to build custom CD jackets._

**For licensing reasons, newer releases do not include a necessary library `hcaenc_lite.dll`.** Please download ADX2LE from its [download page](http://www.adx2le.com/download/index.html), and put
`tools\hcaenc_lite.dll` to DereTore's application directory. If you encounter regional problems, you know there is a way to solve it.

There is a simple [user guide](DereTore.Applications.StarlightDirector/docs/user-guide_zh-CN.md) for Chinese users. Versions in other languages are in progress.

## TODO List

[TODO List](TODO.md)

## Projects

### Core Libraries

**DereTore.Common**

Common code.

**DereTore.StarlightStage**

See it for yourself.

**DereTore.HCA**

The pure C# implementation of CRI HCA v2.0 decoder, based on [kawashima](https://github.com/hozuki/kawashima).
Here is its [readme](DereTore.HCA/README.md).

**DereTore.ACB**

CRI ACB/AWB package manipulation library. Only necessary parts are implemented. Here is its [readme](DereTore.ACB/README.md).

### Applications

Applications can be found in their own directories.

**CipherConverter**

The C# version of HCA cipher conversion utility. See **hcacc** in [hcatools](https://github.com/hozuki/libcgss).

**Encoder**

The C# version of HCA encoding utility. See **hcaenc** in [hcatools](https://github.com/hozuki/libcgss).

**Hca2Wav**

A console appilcation that converts HCA audio to wave audio. It can be used as a demo to integrate
*DereTore.HCA*.

**ACB Maker**

A tool for creating CGSS-compatible ACB archives. Here is its [readme](DereTore.Applications.AcbMaker/README.md).

**Music Toolchain**

The utility that integrates **Encoder**, **CipherConverter** and **AcbMaker** into a streamline. Here
is its [readme](DereTore.Applications.MusicToolchain/README.md).

**Score Viewer**

Viewing scores (beatmaps) of live music. Here is its [readme](DereTore.Applications.ScoreViewer/README.md).

**Starlight Director**

The new score composer (still in alpha phase), the successor of ScoreEditor. Detailed readme and user manual will be written
in the future. Let's celebrate its alpha release!

**ACB Unzip**

An application for unpacking ACB archives.

**Jacket Creator**

A utility for creating CD jackets. Used with score and music makers.

### Other

**Utilities.LZ4**

An LZ4 compression utility for compressing game data. It is designed for IdolProxy.

**Interop.PVRTexLib**

An interop library of [PVRTexLib](https://community.imgtec.com/developers/powervr/graphics-sdk/), for generating PVR textures.

**Interop.D3DX9**

An interop library using D3DX9 to generate DDS textures.

**Interop.UnityEngine**

An library which provides functions to generate CGSS-compatible (for specific Unity version) asset bundles.

## License

This solution uses MIT License. See [LICENSE.md](LICENSE.md).

## Notes

See [here](Notes.md).
