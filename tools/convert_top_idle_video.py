#!/usr/bin/env python3
"""Extract the ReRe idle video into a Unity-ready transparent PNG sequence.

Requires the ComfyUI embedded Python environment with cv2, rembg, and
onnxruntime. Example:

  G:/ComfyUI_flux_workspace/ComfyUI_windows_portable/python_embeded/python.exe \
    tools/convert_top_idle_video.py \
    --input G:/.../job_20260811_204521_d0fd15e9_5s_fast.mp4 \
    --output Assets/Art/ReReSprites/TopIdleVideo

The shared crop is intentionally computed across every selected frame so the
character does not jump around in Unity while the animation loops.
"""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path

import cv2
import numpy as np
from rembg import new_session, remove


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--frames", type=int, default=60)
    parser.add_argument("--fps", type=float, default=12.0)
    parser.add_argument("--model", default="u2net_human_seg")
    parser.add_argument("--max-long-edge", type=int, default=768)
    parser.add_argument("--padding", type=int, default=22)
    parser.add_argument("--keep-existing", action="store_true")
    return parser.parse_args()


def select_frames(video: Path, wanted_frames: int, wanted_fps: float) -> tuple[list[np.ndarray], float]:
    capture = cv2.VideoCapture(str(video))
    if not capture.isOpened():
        raise RuntimeError(f"Unable to open video: {video}")

    source_fps = capture.get(cv2.CAP_PROP_FPS) or wanted_fps
    frame_count = int(capture.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = frame_count / source_fps if source_fps else 0.0
    target_count = min(wanted_frames, max(1, int(round(duration * wanted_fps))))
    indices = np.linspace(0, max(0, frame_count - 1), target_count, dtype=np.int32)

    frames: list[np.ndarray] = []
    last_index = -1
    for index in indices:
        if int(index) == last_index:
            continue
        capture.set(cv2.CAP_PROP_POS_FRAMES, int(index))
        ok, frame = capture.read()
        if not ok:
            raise RuntimeError(f"Unable to read frame {index} from {video}")
        frames.append(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        last_index = int(index)

    capture.release()
    if not frames:
        raise RuntimeError("No source frames were selected.")
    return frames, source_fps


def clean_alpha(mask: np.ndarray) -> np.ndarray:
    """Suppress remaining flat backdrop while retaining semi-transparent edges."""
    if mask.ndim == 3:
        mask = cv2.cvtColor(mask, cv2.COLOR_BGR2GRAY)
    alpha = mask.astype(np.uint8)

    # Keep the main connected foreground component. Tiny isolated remnants from
    # the white/grey video backdrop are discarded, but soft hair stays because
    # rembg's smooth alpha is retained around the component boundary.
    binary = (alpha > 18).astype(np.uint8)
    count, labels, stats, _ = cv2.connectedComponentsWithStats(binary, connectivity=8)
    if count > 1:
        areas = stats[1:, cv2.CC_STAT_AREA]
        main_label = int(np.argmax(areas)) + 1
        main_area = stats[main_label, cv2.CC_STAT_AREA]
        keep = labels == main_label
        for label in range(1, count):
            if label == main_label:
                continue
            area = stats[label, cv2.CC_STAT_AREA]
            # Preserve substantial nearby pieces (a bow/strap can disconnect for
            # a frame) but reject small rectangular background leftovers.
            if area >= max(150, main_area * 0.003):
                keep |= labels == label
        alpha = np.where(keep, alpha, 0).astype(np.uint8)

    # The source has a fixed light rectangle. Never leave a border artifact.
    alpha[:2, :] = 0
    alpha[-2:, :] = 0
    alpha[:, :2] = 0
    alpha[:, -2:] = 0
    return cv2.GaussianBlur(alpha, (0, 0), 0.55)


def shared_crop(alphas: list[np.ndarray], padding: int) -> tuple[int, int, int, int]:
    points = []
    for alpha in alphas:
        ys, xs = np.where(alpha > 10)
        if xs.size:
            points.append((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))
    if not points:
        raise RuntimeError("Foreground extraction produced no opaque pixels.")

    height, width = alphas[0].shape[:2]
    left = max(0, min(point[0] for point in points) - padding)
    top = max(0, min(point[1] for point in points) - padding)
    right = min(width, max(point[2] for point in points) + padding)
    bottom = min(height, max(point[3] for point in points) + padding)
    return left, top, right, bottom


def transparent_outer_pixels(rgba: np.ndarray) -> np.ndarray:
    rgba[:2, :, 3] = 0
    rgba[-2:, :, 3] = 0
    rgba[:, :2, 3] = 0
    rgba[:, -2:, 3] = 0
    return rgba


def main() -> None:
    args = parse_args()
    if not args.input.is_file():
        raise FileNotFoundError(args.input)
    if args.output.exists() and not args.keep_existing:
        shutil.rmtree(args.output)
    args.output.mkdir(parents=True, exist_ok=True)

    frames, source_fps = select_frames(args.input, args.frames, args.fps)
    print(f"Selected {len(frames)} frames from {source_fps:.3f}fps source.")
    print(f"Loading rembg model: {args.model}")
    session = new_session(args.model)

    alphas: list[np.ndarray] = []
    for index, frame in enumerate(frames, 1):
        raw_mask = remove(frame, session=session, only_mask=True, post_process_mask=False)
        alphas.append(clean_alpha(np.asarray(raw_mask)))
        print(f"Matte {index:02d}/{len(frames):02d}")

    left, top, right, bottom = shared_crop(alphas, args.padding)
    crop_width, crop_height = right - left, bottom - top
    scale = min(1.0, float(args.max_long_edge) / max(crop_width, crop_height))
    output_width = max(2, int(round(crop_width * scale)))
    output_height = max(2, int(round(crop_height * scale)))
    if output_width % 2:
        output_width += 1
    if output_height % 2:
        output_height += 1

    for index, (frame, alpha) in enumerate(zip(frames, alphas)):
        rgba = np.dstack((frame, alpha))[top:bottom, left:right]
        if (output_width, output_height) != (crop_width, crop_height):
            rgba = cv2.resize(rgba, (output_width, output_height), interpolation=cv2.INTER_AREA)
        rgba = transparent_outer_pixels(rgba)
        output_path = args.output / f"rere_top_idle_{index:03d}.png"
        cv2.imwrite(str(output_path), cv2.cvtColor(rgba, cv2.COLOR_RGBA2BGRA))

    metadata = {
        "source": args.input.name,
        "source_fps": float(source_fps),
        "playback_fps": float(args.fps),
        "frame_count": len(frames),
        "crop_source_pixels": [int(left), int(top), int(right), int(bottom)],
        "output_size": [int(output_width), int(output_height)],
        "rembg_model": args.model,
    }
    (args.output / "top_idle_video_manifest.json").write_text(json.dumps(metadata, indent=2), encoding="utf-8")
    print(json.dumps(metadata, ensure_ascii=False))


if __name__ == "__main__":
    main()
