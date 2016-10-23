# TODO List

## Score Editor

- [ ] <del>Note hit volume adjustment</del> (not possible)
- [x] Note falling speed adjustment

## Starlight Director

- [x] Advanced composing
  - [x] More detailed division in beats (24/beat)
  - [x] Variant BPM
  - [ ] Different signatures
  - [x] `StartPosition`
    - [x] Translucent indicator layer for Director. (implemented: at the corners)
- [x] Editing
  - [ ] Select area
  - [ ] Copy, cut and paste
  - [ ] Undo and redo
  - [ ] Drag note
  - [ ] Edge scrolling (for inter-measure hold notes)
  - [ ] Auto handling sync notes
  - [ ] Convenient start time offset calibration (e.g. moving the time forward by 1 measure)
  - [x] Binding frequently used keys (PgUp, PgDn, Home, End, etc.)
  - [x] Zooming
    - [x] Auto re-aligning when zooming
    - [x] Quick zooming to the level of seeing 1/8, 1/16, 1/24 note, etc.
  - [x] More efficient rendering (`ScrollViewer`)
    - [ ] Score Lens
- [x] Preview
  - [ ] <del>Live preview</del> (see Deleste)
  - [ ] Inlined audio spectrum
  - [ ] Simple preview audio playback.
  - [x] Changeable sound effect in preview
- [ ] Interop
  - [x] Support for building score database
  - [x] Support for building music archive
  - [ ] <del>Importing existing compiled score(s)</del> (compiled beatmaps cannot be re-imported, but viewing is possible via Deleste)
  - [x] Importing/exporting Deleste beatmaps
  - [x] Save format
    - [x] Auto validation (by primary key)
    - [ ] Metadata (creator, BPM, etc.)
- [ ] Helpers
  - [ ] Globalization
  - [ ] Score image generation
