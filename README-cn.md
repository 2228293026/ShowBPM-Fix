# ShowBPM

[中文](README-cn.md) | [English](README.md)

基于 [ADOFAI_ShowBPM](https://github.com/FLOWERs-Modding/ADOFAI_ShowBPM) 修改，优化了代码可读性并添加了 CI/CD。

## 功能

- **轨道 BPM** — 当前轨道的 BPM
- **实际 BPM** — 考虑速度变化后的有效 BPM
- **每秒按键数** — 基于实际 BPM 的 KPS
- **实际每秒按键数** — 真实的 KPS
- **速度文本** — 编辑器中显示速度倍率
- **忽略同按** — 更准确的实际 BPM 计算

## 构建

```bash
msbuild ShowBPM.sln /p:Configuration=Release
```

`libs/` 中的 DLL 来自游戏安装目录，游戏版本更新后请同步替换。

## 下载

从 [Releases](https://github.com/2228293026/ShowBPM-Fix/releases) 获取最新版本。

## 致谢

- 原始模组作者 [Flower](https://github.com/FLOWERs-Modding/ADOFAI_ShowBPM)
