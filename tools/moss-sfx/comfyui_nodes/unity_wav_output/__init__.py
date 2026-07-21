from __future__ import annotations

import os

import folder_paths
import numpy as np
import soundfile as sf


class SaveUnityWav:
    @classmethod
    def INPUT_TYPES(cls):
        return {
            "required": {
                "audio": ("AUDIO",),
                "filename_prefix": ("STRING", {"default": "moss_sfx"}),
            }
        }

    RETURN_TYPES = ("AUDIO",)
    RETURN_NAMES = ("audio",)
    FUNCTION = "save"
    OUTPUT_NODE = True
    CATEGORY = "audio/Unity"
    DESCRIPTION = "Save 16-bit PCM WAV directly into the configured ComfyUI output directory."

    def __init__(self):
        self.output_dir = folder_paths.get_output_directory()

    def save(self, audio, filename_prefix="moss_sfx"):
        if audio is None:
            raise ValueError("SaveUnityWav: input audio is None.")

        waveform = audio["waveform"].detach().cpu().float()
        sample_rate = int(audio["sample_rate"])
        full_output_folder, filename, counter, subfolder, _ = folder_paths.get_save_image_path(
            filename_prefix, self.output_dir
        )

        results = []
        for batch_index in range(waveform.shape[0]):
            suffix = f"_{batch_index:02}" if waveform.shape[0] > 1 else ""
            file_name = f"{filename}_{counter:05}{suffix}.wav"
            output_path = os.path.join(full_output_folder, file_name)
            samples = np.clip(waveform[batch_index].numpy().T, -1.0, 1.0)
            sf.write(output_path, samples, sample_rate, subtype="PCM_16", format="WAV")
            results.append({"filename": file_name, "subfolder": subfolder, "type": "output"})
            counter += 1

        return {"ui": {"audio": results}, "result": (audio,)}


NODE_CLASS_MAPPINGS = {"SaveUnityWav": SaveUnityWav}
NODE_DISPLAY_NAME_MAPPINGS = {"SaveUnityWav": "Save Unity WAV"}
