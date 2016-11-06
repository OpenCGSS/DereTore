# DereTore.Interop.UnityEngine

This project is used for generating **single-asset** asset bundle for Unity Games.
Initially, it is designed for generating CD jacket bundles for CGSS (which uses Unity 5.1.2f1).

The rebuilding process is based on a brilliant repository [Unity Studio](https://github.com/RaduMC/UnityStudio) and its [fork](https://github.com/Perfare/UnityStudio).
The license of Unity Studio can be found at [License-Unity_Studio.md](License-Unity_Studio.md).

Example usage, with *PVRTexLib*, and *D3DX9*/*DirectXTex*:

```csharp
// using System.Drawing;
// using DereTore.Interop.PVRTexLib;
// using DereTore.Interop.D3DX9;

using (var bmp1 = (Bitmap)Image.FromFile("img-128x128.jpg")) {
    using (var bmp2 = (Bitmap)Image.FromFile("img-264x264.jpg")) {
        var pvr = PvrUtilities.GetPvrTextureFromImage(bmp1);
        var dds = DdsUtilities.GetDdsTextureFromImage(bmp2);
        using (var fs = File.Open("test.unity3d", FileMode.Create, FileAccess.Write)) {
            JacketBundle.Serialize(pvr, 128, 128, dds, 264, 264, 1001, UnityPlatformID.Android, fs);
        }
    }
}
```
