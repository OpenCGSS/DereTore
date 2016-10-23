# DereTore

[![AppVeyor](https://img.shields.io/appveyor/ci/hozuki/DereTore.svg)](https://ci.appveyor.com/project/hozuki/deretore)
[![GitHub contributors](https://img.shields.io/github/contributors/hozuki/DereTore.svg)](https://github.com/hozuki/DereTore/graphs/contributors)
[![Libraries.io for GitHub](https://img.shields.io/librariesio/github/hozuki/DereTore.svg)](https://github.com/houzki/DereTore)
[![Github All Releases](https://img.shields.io/github/downloads/hozuki/DereTore/total.svg)](https://github.com/hozuki/DereTore/releases)

The goal of DereTore is to improve gaming experience in [Idolmaster Cinderella Girls: Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage)
(CGSS/DereSute), or even to customize it a little bit.

**Downloads:**

- [Nightly build](https://ci.appveyor.com/api/projects/hozuki/DereTore/artifacts/deretore-toolkit-x86.zip?job=Platform%3A+x86) (Windows, x86)
- [Releases](https://github.com/hozuki/DereTore/releases)

Wonder [how this name comes from](Notes.md#the-name)?

## Building

1. Clone from [GitHub](https://github.com/hozuki/DereTore.git): `git clone https://github.com/hozuki/DereTore.git`;
2. Install missing NuGet packages: `nuget restore DereTore.sln` (or use NuGet Package Manager in Visual Studio);
3. Open `DereTore.sln` in Visual Studio (VS2010 SP1 or later is required);
4. Build the solution.

## Usage

Requirements:

- Windows 7 or later (though some tools should be able to run on Mono)
- .NET Framework 4.0

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

The pure C# implementation of CRI HCA v2.0 decoder, based on [kawashima](https://github.com/Hozuki/kawashima).
Here is its [readme](DereTore.HCA/README.md).

**DereTore.ACB**

CRI ACB/AWB package manipulation library. Only necessary parts are implemented. Here is its [readme](DereTore.ACB/README.md).

### Applications

**CipherConverter**

The C# version of HCA cipher conversion utility. See **hcacc** in [hcatools](https://github.com/Hozuki/hcatools).

**Encoder**

The C# version of HCA encoding utility. See **hcaenc** in [hcatools](https://github.com/Hozuki/hcatools).

**Hca2Wav**

A console appilcation that converts HCA audio to WAVE audio. It is similar to DereTore.HCA.Test,
but it outputs to a stream, in fixed linear sequence. It can be used as a clearer demo to integrate
DereTore.HCA.

**AcbMaker**

A tool for creating CGSS-compatible ACB archives. Here is its [readme](DereTore.Applications.AcbMaker/README.md).

**MusicToolchain**

The utility that integrates **Encoder**, **CipherConverter** and **AcbMaker** into a streamline. Here
is its [readme](DereTore.Applications.MusicToolchain/README.md).

**ScoreEditor**

Viewing and editing scores (collections of notes) of live music. Here is its [readme](DereTore.Applications.ScoreEditor/README.md).

**StarlightDirector**

The new score composer (still in alpha phase), the successor of ScoreEditor. Detailed readme and user manual will be written
in the future. Let's celebrate its alpha release!

**AcbUnzip**

An application that unpacks ACB archives.

### Other

**LZ4**

An LZ4 compression utility for compressing game data. It is designed for IdolProxy.

## License

This solution uses **_modified_** [MIT License](http://mit-license.org/). See its [license file](LICENSE.md).

## Notes

See [here](Notes.md).
