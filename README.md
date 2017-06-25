# DereTore

**IMPORTANT: beginning from version 3.0.3, CGSS is compiled by IL2CPP, which may cause some of the applications in this repository not being able to work. We are struggling with it.**

[![AppVeyor](https://img.shields.io/appveyor/ci/hozuki/deretore-avoh8.svg)](https://ci.appveyor.com/project/hozuki/deretore-avoh8)
[![GitHub contributors](https://img.shields.io/github/contributors/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore/graphs/contributors)
[![Libraries.io for GitHub](https://img.shields.io/librariesio/github/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore)
[![Github All Releases](https://img.shields.io/github/downloads/OpenCGSS/DereTore/total.svg)](https://github.com/OpenCGSS/DereTore/releases)

The goal of DereTore is to improve gaming experience in [Idolmaster Cinderella Girls: Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage)
(CGSS/DereSute), and even to customize it a little bit.

**Downloads:**

- [Nightly build](https://ci.appveyor.com/api/projects/hozuki/deretore-avoh8/artifacts/deretore-toolkit-x86.zip?job=Platform%3A+x86) (Windows, x86)
- [Releases](https://github.com/OpenCGSS/DereTore/releases)

A newer version of Starlight Director (beatmap editor) can be found [here](https://github.com/hozuki/StarlightDirector). However, if you
want to build ACB music, currently you are still advised to use `MTC` in this repository.

Wonder [how this name comes from](docs/Notes.md#the-name)?

## What can it do?

Create beatmaps, convert music - which are playable in vanilla CGSS. You can also unpack and decode resources from the game.

There are several [projects](docs/Projects.md) in DereTore. Each of them provides a unique function.

## Usage

[中文使用说明](StarlightDirector/StarlightDirector/docs/user-guide_zh-CN.md)

**Basic requirements:**

- Windows 7 or later
- [.NET Framework 4.5](https://www.microsoft.com/en-us/download/details.aspx?id=42642)

**Optional requirements:**

If you want to build custom CD jackets:

- [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784)
- [DirectX 9.0c](https://www.microsoft.com/en-us/download/details.aspx?id=8109)

To use Starlight Director your system has to support Direct2D.

**For licensing reasons, newer releases do not include an essential library `hcaenc_lite.dll`.** However, you can:

1. download `hcaenc_lite.dll` from [here](https://mega.nz/#!QxQjnZRB!85k5O6K5oMMM1W9ux7ZpkzXQFgV4EoYplZsW1ZOWZnM), or
2. download ADX2LE from its [download page](http://www.adx2le.com/download/index.html), and put `tools\hcaenc_lite.dll` to DereTore's application directory.
If you encounter regional problems, you know there is a way to solve it.

## Building

1. Clone from [GitHub](https://github.com/OpenCGSS/DereTore.git): `git clone https://github.com/OpenCGSS/DereTore.git`;
2. Install missing NuGet packages: `nuget restore DereTore.sln` (or use NuGet Package Manager in Visual Studio);
3. Open `DereTore.sln` in Visual Studio (Visual Studio 2015 or later is required for supporting C# 6 syntax);
4. Build the solution.

## TODO List

[TODO List](docs/TODO.md)

## License

This solution uses MIT License. See [LICENSE.md](LICENSE.md).

## Notes

See [here](docs/Notes.md).
