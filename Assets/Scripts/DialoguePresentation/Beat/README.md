# Beat preset

`BeatCommand` adds the explicit Naninovel command `@beat type:<name>`. The
command resolves the name in a `BeatLibrary` assigned to a scene or persistent
`BeatController` component. If Naninovel is not initialized, the controller is
missing, the library is missing, or the type is unknown, the command completes
without querying services or throwing. Optional clips and visuals may remain
unassigned.

Use `MoshiReRe/Dialogue Presentation/Beat/Create Default Library` to create
`Assets/Resources/DialoguePresentation/Beat/DefaultBeatLibrary.asset` with the
`awkward`, `shock`, `realize`, and `punchline` presets. The menu only adds a
missing type, so later runs do not overwrite tuned values. Assign that asset to
`BeatController`; add the controller to an existing persistent gameplay object.

Each preset controls duration, scaled/unscaled timing, optional SFX, flash and
blackout alpha, temporary text-printer visibility, light main-camera shake, and
whether to wait for Naninovel Continue input. When no `BeatOverlay` is assigned,
the controller creates a raycast-free Screen Space Overlay only when a beat
needs one. A missing camera or audio source simply disables that part.

When a new beat starts, the previous beat is cancelled and its owned effects
are restored before the new one is applied. While Naninovel skip mode is active,
the visual/audio effect is applied but duration and input wait are reduced to
zero. Cancellation from a later beat is treated as normal completion; a real
Naninovel cancellation token is still propagated.

## Naninovel example

See `Examples/BeatExample.nani`.
