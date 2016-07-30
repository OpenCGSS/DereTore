# DereTore.Application.AcbMaker

ACBs are used to store audio and images in CGSS. This application can generate
CGSS-compatible ACB files. Currently it only supports single live music file
(in HCA format) as its content.

Usage:

```cmd
AcbMaker <output ACB> <HCA live music file> [-n <song name>]
```

The default value of `<song name>` is `"song_1001"`.

Example:

```cmd
AcbMaker output.acb song_1004_conv.hca -n song_1004
```

------

**Acknowledgements:**

[@hcs64](https://github.com/hcs64) for his [utf_view](https://www.hcs64.com/vgm_ripping.html) utility used in format
verification.

[@hyspace](https://github.com/hyspace) for playback injection tips.
