
![Header Animation](https://camo.githubusercontent.com/a63fd9333526efd7570e67b68bb7d40828e4f9abc865b43c0a4417b46be19554/68747470733a2f2f63617073756c652d72656e6465722e76657263656c2e6170702f6170693f747970653d776176696e67266865696768743d31303026636f6c6f723d6772616469656e74)

> **STATUS UPDATE**
> I am **working** on SqTS. Updates are on the way. I just haven't had much opportunities to code in the past week. You can expect 2+ updates for the next week tho.

> **Want to showcase SqTS?**
> Have you made a video of **SqTS** in action? I’d love to see it!
> You can create a issue with the **showcase** tag and i will personally review and add your video to this repo.

> ***Tip:** To list installed voices, just run the program with the "-list-voices" parameter.*

# SqTS Real-time S2S Engine
> ***Fun fact:** SqTS stands for S quadruple T S, which just means STT>TTS.*

SqTS Is a fully free and open source S2S engine made entirely in C# and .NET
SqTS is fully **RAM-only** meaning it doesnt write to your disk when doing its thing.
That means that your data is there as long as you dont interrupt the **DRAM power cycle** or **power the hardware off**.

## Features
 - Filtering engine that supports commands like **EQUALS**, **CONTAINS**, **STARTS**, **ENDS** and **REGEX**.
 - Fully automatic STT model downloads.
 - Fully configurable using the configuration files.
 - Fully compatible with most hardware. *(compatible with **CPU**, **Cuda**, **CoreML**, **Metal**, **NoAVX**, **OpenVino** and **Vulkan** thanks to the **Whisper.net.AllRuntimes** package.)*

### System Requirements
| Minimum | Recommended |
|---|---|
| A fast x64 CPU. | 6+ Core x64 CPU with AVX2. |
| No GPU required. | A NVidia RTX 2K+ GPU (8+ GBs of VRAM) |
| 4 GBs of RAM. | 16 GBs of RAM. |
| 500+ MBs of storage. (Tiny Model) | 8+ GBs of storage. (Any Model) |

# Filtering Engine

> **STATUS UPDATE**
> The filtering engine is being **rewritten.** Expect this to change once the filtering engine is fully rewritten.

## Footer
[![Star History Chart](https://api.star-history.com/chart?repos=davpnuser/SqTS-S2S&type=date&legend=top-left)](https://www.star-history.com/?repos=davpnuser%2FSqTS-S2S&type=date&legend=top-left)
![Footer Animation](https://camo.githubusercontent.com/62113ebf2951e037bfed07e38dace16f3f5c049df5a613bf9c4caddde55a3c31/68747470733a2f2f63617073756c652d72656e6465722e76657263656c2e6170702f6170693f747970653d776176696e67266865696768743d31303026636f6c6f723d6772616469656e742673656374696f6e3d666f6f746572)
