# ShowBPM

[中文](README-cn.md) | [English](README.md)

Fork of [ADOFAI_ShowBPM](https://github.com/FLOWERs-Modding/ADOFAI_ShowBPM) with code readability improvements and CI/CD.

## Features

- **Tile BPM** — BPM of the current tile
- **Real BPM** — effective BPM considering speed changes
- **KPS** — keys per second based on real BPM
- **Next BPM** — BPM of the next upcoming speed change
- **Real KPS** — actual keys per second
- **Speed Text** — show speed multipliers in editor
- **Customizable Order** — freely arrange the display order of BPM lines
- **Text Alignment** — left/center/right alignment with correct anchoring
- **Ignore Multipress** — better real BPM calculation for pseudos

## Build

```bash
msbuild ShowBPM.sln /p:Configuration=Release
```

DLLs in `libs/` are from the game installation. Update them if your game version differs.

## Download

Get the latest release from [Releases](https://github.com/2228293026/ShowBPM-Fix/releases).

## Credits

- Original mod by [Flower](https://github.com/FLOWERs-Modding/ADOFAI_ShowBPM)
