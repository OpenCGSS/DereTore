# DereTore.Application.Toolchain

This project is meant for integrating the 3 tools: **AcbMaker**, **CipherConverter** and **Encoder**.
It provides an automatic streamline to create CGSS-compatible live music ACB files.

In order to run the application, the executables of those tools must be placed under the directory
containing the executable of this project. Additionally, the `hcaenc_lite.dll` must also be placed
there, as it is a prerequisite for **Encoder**. Go to the [download page](http://adx2le.com/download/index.html)
for it. Remember, it is only for domestic use in Japan, but it is not so difficult to get a copy.

## How to use the utility

1. Select a 16-bit WAVE file.
2. Fill in the cipher keys to generate an dynamically encrypted (type 56) HCA file, or left both textboxes
blank to generate a statically encrypted (type 1) HCA file. They are filled with CGSS cipher keys by default.
3. Fill in a song name. The name shall look like `song_####` where `#`s are digits. An example is already
filled in by default.
4. Select a save location.
5. Press the 'Go' button.
