# Exploration walk-cycle source

`walking_frame.psb` is the user-provided roomwear walk cycle.

- Document size: 480 x 624 pixels
- Layer order: `frame_0001`, `frame_0003`, `frame_0006`, `frame_0009`,
  `frame_0012`, `frame_0015`, `frame_0018`, `frame_0021`,
  `frame_0024`, `frame_0027`, `frame_0030`, `frame_0033`
- Derived runtime sheet:
  `Assets/Art/ExplorationPrototype/FrameAnimation/player_casual_walk_video_12.png`
- Every runtime frame keeps the full 480 x 624 document canvas and a bottom-center pivot.
  This preserves the source alignment and prevents per-frame position jitter.
- The derived frames are mirrored once so the unflipped SpriteRenderer faces screen-right,
  matching `ExplorationSpriteAnimator.SetFacingRight`.
- Runtime playback is 12 frames per second; frames 3 and 9 are the two default-outfit
  contact poses used when coming to rest.
- On input release, playback advances to the nearer of the two contact poses before
  entering a subtle procedural breathing motion. This avoids snapping directly from
  an arbitrary stride frame to a fixed idle sprite.
- A separate, high-contrast `PlayerGroundShadow` sprite follows the player below the character layer.
  It is shared by both outfits and is not baked into the walk frames.

The wardrobe suit currently continues to use its existing eight-frame sprite sequence.
It can be replaced by another fixed-canvas video-derived sequence without changing the runtime component.
