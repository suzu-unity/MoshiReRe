---
name: generate-moshirere-audio
description: Generate BGM, SE/SFX, and ambience for the MoshiReRe Unity project with local MiniMax Music 3 or MOSS. Use when a user asks in Japanese or English to create, iterate, or replace BGM, background music, 効果音, SE, SFX, or 環境音.
---

# MoshiReRe Audio Generation

## Route the request

- Use **MiniMax Music 3** for BGM: melodic, rhythmic, harmonic, long-form, or scene music. Save it to `Assets/Audio/BGM/Generated`.
- Use **MOSS SoundEffect v2** for a discrete SE/SFX: UI feedback, footsteps, impacts, foley, short actions, or an isolated sound. Follow `.agents/skills/generate-moss-sfx/SKILL.md` and save it to `Assets/Audio/SFX/Generated`.
- Treat a non-musical diegetic ambience, such as rain, office room tone, station noise, or a crowd bed, as MOSS. Treat an evolving atmospheric music bed with harmony, melody, pulse, or an intended BGM role as Music 3.
- For a mixed request, generate the music and diegetic ambience as separate assets. If the requested role is unclear, ask whether it should be music or a loopable environmental sound.
- Do not queue MOSS and Music 3 concurrently on the RTX 4070.

## Start the selected environment

For Music 3, run `tools/music3/Start-Music3Environment.ps1`. It validates the shared model configuration, starts `G:\ComfyUI_Flux_Workspace\ComfyUI_Music3\launch_music3.py` only when `http://127.0.0.1:8190` is not already healthy, and starts a separate Comfy Agent bridge on port `9181`. It never stops an existing process. Open `http://127.0.0.1:8190` and use the Agent panel Connect action when the bridge is needed.

For MOSS, use the existing workflow and endpoint `http://127.0.0.1:8188`; do not retarget a live MOSS connection to Music 3. Before queuing either workflow, confirm its own `/system_stats` endpoint is healthy.

## Generate Music 3 BGM

1. Open the official template at `G:\ComfyUI_Flux_Workspace\ComfyUI_Music3\python_overlay\comfyui_workflow_templates_json\templates\audio_minimax_music_3.json`.
2. In its `Text to Music (MiniMax Music 3)` node, use the RTX 4070 defaults:
   - `unet_name`: `minimax_music3_dit_int8_convrot.safetensors`
   - `clip_name`: `minimax_music3_text_encoder_pruned_int8_convrot.safetensors`
   - `vae_name`: `minimax_music3_dav.safetensors`
   - `tiled_decode`: `true`
3. Translate the Japanese request into natural English. Write `caption` in these three named parts:
   - `Global Metadata`: explicit genre, feel, BPM, key, and meter, then the scene, energy arc, loop intent, and exclusions.
   - `Vocal Details`: register, timbre, breathiness, articulation, delivery, doubling, and mix placement. State vocal exclusions directly when important.
   - `Arrangement`: describe the instruments and their section-by-section behavior in sync with the lyric tags (for example, sparse verse, lift in pre-chorus, reduced bridge, final chorus, ending).
   Do not use an artist, band, or named-song reference to request imitation. Decompose the desired sound into musical characteristics instead. For instrumental BGM, state `instrumental only, no vocals, no spoken word` and use `[Instrumental]` for lyrics.
4. Write lyrics with structural tags such as `[Intro]`, `[Verse]`, `[Pre-Chorus]`, `[Chorus]`, `[Bridge]`, and `[Outro]`; use the same sections in `Arrangement`. Put short backing-vocal, humming, or intentional-space cues in parentheses on their own lyric lines, such as `(ooh)` or `(brief pause)`. Treat those as performance cues that may be sung or interpreted by the model, not as guaranteed literal spoken text.
5. Use a fixed seed supplied by the user or record the generated seed. Start an audition at 30 seconds when VRAM risk is unknown; use the template 60-second duration for a final short BGM. `max_duration` is an upper bound, not a promised runtime: Music 3 may end earlier when the musical structure resolves.
6. Set `SaveAudioAdvanced.filename_prefix` to a unique short ASCII snake_case name without a directory. Keep the template MP3 output unless the user requests another supported format. ComfyUI numeric suffixes preserve variants; never overwrite a named asset without permission.

The Music 3 server is launched with `Assets/Audio/BGM/Generated` as its output directory. Verify that the new audio file stays below that directory, is non-empty, and imports in Unity. Use `ffprobe` to report duration, sample rate, and channels when it is available; otherwise report that Unity successfully imported the asset.

Record and report the English caption, lyrics, seed, duration, the three model names, `tiled_decode`, template path, format, and absolute output path. Keep a sibling generation JSON only when the user asks for a persistent reproduction record.

## Music 3 boundaries

- Music 3 is text-to-music. It does not make a supplied reference song into a style transfer or perform audio inpainting; use a separate compatible tool/workflow for those tasks.
- Never request imitation through the name of a living artist or other specific performer. Describe genre, rhythm, harmony, vocal technique, production, and arrangement instead.

## Recover safely

- If port `8190` is occupied but `/system_stats` is not healthy, stop and report the PID; do not kill it.
- If any Music 3 model is absent from the node dropdown, verify `G:\ComfyUI_Flux_Workspace\ComfyUI_Music3\extra_model_paths.yaml` and the three shared model files before restarting Music 3.
- For CUDA out-of-memory failures, keep the int8 models and tiled decode, reduce duration to 30 seconds, and retry once after the other audio workflow is idle.
- For a failed queue or invalid output, inspect the Music 3 ComfyUI logs/history and choose a new suffix rather than replacing an existing generated file.
