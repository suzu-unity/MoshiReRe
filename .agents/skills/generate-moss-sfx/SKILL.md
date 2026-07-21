---
name: generate-moss-sfx
description: Generate sound effects for the MoshiReRe Unity project with local OpenMOSS MOSS-SoundEffect v2.0 through the ComfyUI/Comfy Agent MCP integration. Use when the user asks to create, generate, synthesize, iterate, or replace an SE/SFX/audio effect for the game, including Japanese requests such as 効果音, SE, or 音を作って.
---

# Generate MOSS SFX

Use the `comfyui` MCP server and the MOSS SoundEffect v2 workflow. Save finished WAV files under `Assets/Audio/SFX/Generated`.

## Workflow

1. Check ComfyUI at `http://127.0.0.1:8188`. If unavailable, start `tools/moss-sfx/Start-MossComfy.ps1` in a hidden background PowerShell process and wait for `/system_stats`.
2. Confirm these installed node classes are available:
   - `MOSS_SoundEffectV2Loader`
   - `MOSS_SoundEffectV2Generate`
   - `SaveUnityWav`
3. Open a new empty workflow tab. Do not add the MOSS branch to an existing workflow: ComfyUI validates unrelated missing nodes even when queuing only the audio output.
4. Load the custom-node template named `MOSS SoundEffect v2`, replace its deprecated `SaveAudio` node with `SaveUnityWav`, or construct the same three-node graph if template lookup is unavailable.
5. Translate Japanese descriptions into precise natural English before sending the model prompt. Preserve the requested action, material, distance, space, intensity, and exclusions. MOSS v2 is trained for English and Chinese, not Japanese.
6. Set generation values from the request. Use these defaults when unspecified:
   - seconds: `3.0`
   - num_inference_steps: `100`
   - cfg_scale: `4.0`
   - sigma_shift: `5.0`
   - seed: random
   - append_duration_suffix: `true`
   - loader weight_quantization: `auto`
   - loader disable_torch_compile: `true`
7. Set `SaveUnityWav.filename_prefix` to a short ASCII snake_case asset name. Do not include a directory; ComfyUI already writes into the Unity `Generated` folder.
8. Queue the workflow and wait for completion. The first run may download about 11.2 GB and take much longer.
9. Verify that a new `.wav` exists under `Assets/Audio/SFX/Generated`, is non-empty, and is 48 kHz. Report the absolute file link, actual duration, seed, and English prompt.

Do not overwrite an existing audio file unless the user explicitly asks. Let ComfyUI add its numeric suffix for variants. If generation fails from CUDA memory pressure, free ComfyUI models and retry once with `weight_quantization=int8_convrot`; report the failure if that path is unavailable.
