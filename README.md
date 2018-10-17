# DereTore

[![AppVeyor](https://img.shields.io/appveyor/ci/hozuki/deretore-avoh8.svg)](https://ci.appveyor.com/project/hozuki/deretore-avoh8)
[![GitHub contributors](https://img.shields.io/github/contributors/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore/graphs/contributors)
[![Libraries.io for GitHub](https://img.shields.io/librariesio/github/OpenCGSS/DereTore.svg)](https://github.com/OpenCGSS/DereTore)
[![Github All Releases](https://img.shields.io/github/downloads/OpenCGSS/DereTore/total.svg)](https://github.com/OpenCGSS/DereTore/releases)
![GitHub (pre-)release](https://img.shields.io/github/release/hozuki/MilliSim/all.svg)

The goal of DereTore is to improve gaming experience in [THE iDOLM@STER Cinderella Girls: Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage)
(CGSS/DereSute/デレステ), and even to customize it a little bit.

**Downloads:**

- [Nightly build](https://ci.appveyor.com/api/projects/hozuki/deretore-avoh8/artifacts/deretore-toolkit-latest.zip) (Windows)
- [Releases](https://github.com/OpenCGSS/DereTore/releases)

A newer version of Starlight Director (the beatmap editor) can be found at [hozuki/StarlightDirector](https://github.com/hozuki/StarlightDirector). However, if you
want to build ACB music, currently you are still advised to use Music Toolchain in this repository.

Wonder [how this name comes from](docs/Notes.md#the-name)?

## What can it do?

- Extract audio files used by CGSS (and some other games using ACB/HCA).
- Decode those audio files to wave audio.
- Create songs, and their cover images.
- Create beatmaps.
- Preview beatmaps.

The greatest thing is that the things you create is fully playable. You just need to
replace the original files, or use IdolProxy (which is easier).

Check out [projects](docs/Projects.md) for detailed information.

## Usage

Have a look at the [Wiki Page](https://github.com/OpenCGSS/DereTore/wiki). Please contact us if you want to help.

**Basic requirements:**

Windows:

  - Windows 7 or later
  - [.NET Framework 4.5](https://www.microsoft.com/en-us/download/details.aspx?id=42642)
  - OpenAL (bundled [OpenAL-Soft](https://github.com/kcat/openal-soft) builds in newer releases)

macOS/Linux:

  - [Wine](https://www.winehq.org/download) (will install wine-mono when needed)
  - OpenAL (bundled [OpenAL-Soft](https://github.com/kcat/openal-soft) builds in newer releases)

In newer releases, HCA encoding is provided by [VGAudio](https://github.com/Thealexbarney/VGAudio) so `hcaenc_lite.dll` is no longer needed.

**Optional requirements:**

If you want to build custom CD jackets:

- [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40784)
- [DirectX 9.0c Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109)

Checked Feb. 09, 2018: Although JacketCreator generates asset bundles for Unity version 5.1.2f1 (original version that CGSS uses),
CGSS (using Unity 5.4.5p1) is still able to load and display them.
<del>Cygames also updated the Unity version they used, so maybe the jackets created by Jacket Creator become unrecognizable.</del>

## Building

1. Clone from [GitHub](https://github.com/OpenCGSS/DereTore.git): `git clone https://github.com/OpenCGSS/DereTore.git`;
2. Restore all NuGet packages;
3. Open `DereTore.sln` in Visual Studio (Visual Studio 2017 or later is required for supporting C# 7 syntax);
4. Build the solution.

## Contributing

Beginning from version 3.0.3, CGSS is compiled by IL2CPP. This may cause some problems in the future.
If you feel like to track the latest changes, feel free to make a pull request.

[TODO List](docs/TODO.md)

## License

This solution uses MIT License. See [LICENSE.md](LICENSE.md).

## Notes

See [here](docs/Notes.md).
