# DereTore

The open-source components of DereTore. The goal of DereTore is to improve experience in
[The Idolmaster Cinderella Girls Starlight Stage](http://www.project-imas.com/wiki/THE_iDOLM@STER_Cinderella_Girls%3A_Starlight_Stage).

Wonder [how this name comes from](#the-name)?

## Projects

**DereTore.HCA**

The pure C# implementation of CRI HCA v2.0 decoder, ported from [kawashima](https://github.com/Hozuki/kawashima).
Here is its [readme](DereTore.HCA/README.md).

**DereTore.HCA.Native**

The C# wrapper of [kawashima](https://github.com/Hozuki/kawashima). Here is its [readme](DereTore.HCA.Native/README.md).

**DereTore.HCA.Test**

The console test application for DereTore.HCA.

**DereTore.HCA.Native.Test**

The console test application for DereTore.HCA.Native.

**DereTore.ACB**

CRI ACB/AWB package extraction library. Only necessary parts are implemented. Here is its [readme](DereTore.ACB/README.md).

**DereTore.ACB.Test**

The console test application for DereTore.ACB.

**DereTore.Application.CipherConverter**

The C# version of HCA cipher conversion utility. See **hcacc** in [hcatools](https://github.com/Hozuki/hcatools).

**DereTore.Application.Encoder**

The C# version of HCA encoding utility. See **hcaenc** in [hcatools](https://github.com/Hozuki/hcatools).

## License

This project uses [MIT License](http://mit-license.org/). See its [license file](LICENSE.md).

## Notes

### The Name

The name DereTore (デレトレ) is the short form of "Cinderella Trainer" (シン**デレ**ラ **トレ**イナー), in response to the
official abbreviation of CGSS, DereSute (デレステ, アイドルマスター シン**デレ**ラ ガールズ スターライト **ステ**ージ).

### The History

DereTore was at first targetted to Unity3D, creating a mini game that can play live music as in CGSS. That's why the language levels
in `DereTore.HCA` and `DereTore.ACB` are C# 4.0, the latest C# version supported by Unity3D. Other projects serve as utilities
or tests, therefore restrictions on those projects are fewer.

Its new goal is to improve the gaming experience, adding features to the original game. Now, the projects are more like a CGSS toolkit
written in C#.

Early versions of DereTore apply [WTFPL](http://www.wtfpl.net/) for most of the components. However, newer versions embrace MIT License.
