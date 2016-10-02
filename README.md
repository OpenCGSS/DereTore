# DereTore

[![Build Status](https://travis-ci.org/hozuki/DereTore.svg?branch=master)](https://travis-ci.org/hozuki/DereTore)
[![Build status](https://ci.appveyor.com/api/projects/status/08drntgkuv3vxtom?svg=true)](https://ci.appveyor.com/project/hozuki/deretore)

The goal of DereTore is to improve gaming experience in [The Idolmaster Cinderella Girls Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage)
(CGSS), or even to customize it a little bit.

Wonder [how this name comes from](#the-name)?

## Building

1. Clone from [GitHub](https://github.com/hozuki/DereTore.git): `git clone https://github.com/hozuki/DereTore.git`;
2. Install missing NuGet packages: `nuget restore DereTore.sln`;
3. Open `DereTore.sln` in Visual Studio (VS2010 SP1 or later is required);
4. Build the solution.

## Projects

### Common Libraries

**DereTore.Common**

Common code.

**DereTore.StarlightStage**

See it for yourself.

### Main Libraries and Tests

**DereTore.HCA**

The pure C# implementation of CRI HCA v2.0 decoder, based on [kawashima](https://github.com/Hozuki/kawashima).
Here is its [readme](DereTore.HCA/README.md).

**DereTore.HCA.Native**

The C# wrapper of [kawashima](https://github.com/Hozuki/kawashima). Here is its [readme](DereTore.HCA.Native/README.md).

**DereTore.HCA.Test**

The console test application for DereTore.HCA. It starts an audio preview for HCA files.

**DereTore.HCA.Native.Test**

The console test application for DereTore.HCA.Native.

**DereTore.ACB**

CRI ACB/AWB package manipulation library. Only necessary parts are implemented. Here is its [readme](DereTore.ACB/README.md).

**DereTore.ACB.Test**

The console test application for DereTore.ACB.

### Applications

**DereTore.Applications.CipherConverter**

The C# version of HCA cipher conversion utility. See **hcacc** in [hcatools](https://github.com/Hozuki/hcatools).

**DereTore.Applications.Encoder**

The C# version of HCA encoding utility. See **hcaenc** in [hcatools](https://github.com/Hozuki/hcatools).

**DereTore.Applications.Hca2Wav**

A console appilcation that converts HCA audio to WAVE audio. It is similar to DereTore.HCA.Test,
but it outputs to a stream, in fixed linear sequence. It can be used as a clearer demo to integrate
DereTore.HCA.

**DereTore.Applications.AcbMaker**

A tool for creating CGSS-compatible ACB archives. Here is its [readme](DereTore.Applications.AcbMaker/README.md).

**DereTore.Applications.MusicToolchain**

The utility that integrates **Encoder**, **CipherConverter** and **AcbMaker** into a streamline. Here
is its [readme](DereTore.Applications.MusicToolchain/README.md).

**DereTore.Applications.ScoreEditor**

Viewing and editing scores (collections of notes) of live music. Here is its [readme](DereTore.Applications.ScoreEditor/README.md).

**DereTore.Applications.StarlightDirector**

The new score composer (still in alpha phase), the successor of ScoreEditor. Detailed readme and user manual will be written
in the future. Let's celebrate its alpha release!

There is a simple user guide in Simplified Chinese: [here](DereTore.Applications.StarlightDirector/docs/user-guide_zh-CN.md).

### Other

**DereTore.Utilities.LZ4**

An LZ4 compression utility for compressing game data. It is designed for IdolProxy.

## License

This solution uses **_modified_** [MIT License](http://mit-license.org/). See its [license file](LICENSE.md).

## Notes

### The Name

The name DereTore (デレトレ) is the short form of "Cinderella Trainer" (シン**デレ**ラ **トレ**イナー), in response to the
official abbreviation of CGSS, DereSute (デレステ), which is the short form of the game title (アイドルマスター シン**デレ**ラ ガールズ スターライト **ステ**ージ).

### The History

DereTore was at first targetted to Unity3D, creating a mini game that can play live music as in CGSS. That's why the language levels
in `DereTore.HCA` and `DereTore.ACB` were C# 4.0, the last C# version supported by Unity3D. Other projects serve as utilities
or tests, therefore restrictions on those projects are fewer. Now that the solution targets Windows PC, all the projects use the
latest C# version.

Its new goal is to improve the gaming experience, adding features to the original game. Now, the projects are more like a CGSS toolkit
written in C#.

Early versions of DereTore apply [WTFPL](http://www.wtfpl.net/) for most of the components. However, newer versions embrace MIT License.

### About the Word "score"

CGSS is a music game, and its basic interacting element is **note**. The word **score** is used to reference the full collection of
notes in a single gameplay, a single song. So it is more likely to be a term, rather than scores for composers or matches. See the
[music video game](https://en.wikipedia.org/wiki/Music_video_game) entry on Wikipedia. Still, I don't know why the developers from
Cygames use "atapon", which is the abbreviation of [a song](http://www.project-imas.com/wiki/Atashi_Ponkotsu_Android), all over the code.

### Customizing CGSS

Please keep in mind that cheating is notorious, and tearing the game apart is illegal. So please, DO NOT transmit evil data, and DO
keep the game in its original state.
