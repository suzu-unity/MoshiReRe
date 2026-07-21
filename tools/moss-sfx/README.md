# MOSS SoundEffect v2 / Comfy Agent

This project uses a dedicated local ComfyUI installation at
`C:\Users\suzu\ComfyUI-MOSS`. Generated WAV files are written directly to
`Assets\Audio\SFX\Generated`, where Unity imports them automatically.

## Start

Run `Start-MossComfy.cmd`. It starts ComfyUI and the Comfy Agent orchestrator,
then opens `http://127.0.0.1:8188`.

The ComfyUI sidebar contains **Agent**. Choose **ChatGPT**, connect with the
Codex login, and request a sound effect. Example:

> MOSS SoundEffect v2で、木製ドアを静かに閉める音を2秒、seed 42で生成して。
> WAVを `door_close_soft` という接頭辞で保存して。

Use the **Save Unity WAV** node for output. The stock ComfyUI **Save Audio**
node writes FLAC and is not the Unity output path used by this project.
Start each sound request in a new empty workflow tab so unrelated missing nodes
from another workflow cannot block audio generation.

Codex Desktop can also use the `comfyui` MCP server after Codex is restarted.
Keep ComfyUI running before requesting generation.

## Recommended settings

- Duration: 0.1–30 seconds
- Steps: 100
- CFG: 4.0
- Sigma shift: 5.0
- Loader: `weight_quantization=auto`, `disable_torch_compile=true`
- Output: 48 kHz WAV

The first generation downloads the OpenMOSS model (about 11.2 GB) and is much
slower than later generations.
