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
- Runtime playback is 12 frames per second; frame 3 is used while idle.
- A separate `PlayerGroundShadow` sprite follows the player below the character layer.
  It is shared by both outfits and is not baked into the walk frames.

The wardrobe suit currently continues to use its existing eight-frame sprite sequence.
It can be replaced by another fixed-canvas video-derived sequence without changing the runtime component.
