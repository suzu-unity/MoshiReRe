# MoshiReRe – AI Development Guide

MoshiReRe is a Unity visual novel built with Naninovel. It uses menu-driven UI,
script-based gameplay, and small systems for money, inventory, and advice.
Gameplay code is under `Assets/Scripts`.

Before discussing or implementing the ReRe app, the nighttime "itadaki" loop,
character information nodes, debt deadlines, or the daytime company-game loop,
read `シナリオ候補/00_ゲーム概形_頂き女子と会社パート.md`.

## Systems

- `MoneySystem`: `MoneyManager` is the money singleton and raises
  `OnMoneyChanged`; `MoneyUI` displays the amount, animated changes, and sound.
- `MenuSystem`: `MenuRootUI` switches pages, `MenuEsc` toggles with ESC, and
  the `Advice*` scripts provide hover/click context help.
- `LocationHUD`: displays the current location.
- `LocationCard`: `LocationCardUI` displays location cards.
- `Data`: `CharacterDatabase`/`CharacterInfo` and
  `InventoryDatabase`/`InventoryItem` hold character and item definitions.

Systems are intentionally small, UI-driven, and event-oriented.

## Project rules

- Prefer small Unity components, event-driven communication, UI logic in UI
  scripts, and data in `Assets/Scripts/Data`.
- Avoid heavy architecture frameworks and unnecessary refactoring.
- Preserve public APIs, serialized fields, inspector compatibility, and Unity
  friendliness. Prefer the smallest extension or a new focused script; keep
  systems and event flows separate.
- For gameplay changes, search `Assets/Scripts` first, identify the relevant
  system, and read only the files needed for the change.
- Ignore `Library`, `Temp`, `Logs`, `Build`, and `obj`; they are generated and
  are outside normal project inspection.
- Preserve and report unrelated user changes; never reset or overwrite them.
  Commit only the assigned changes after their required verification passes.
- Make routine decisions from existing conventions; escalate consequential
  specification, design, or data-loss decisions instead of guessing.

Typical entry points are `MenuRootUI`, `MoneyManager`, `AdviceTrigger`, and
`LocationHUD`.

## Efficient Unity work

For Unity Editor or Unity MCP work, or whenever hierarchy, console, screenshots,
domain reloads, or repeated tool verification are involved, load and follow
`.agents/skills/token-efficient-unity/SKILL.md`. It contains the detailed
cost-aware workflow, capability checks, delegation guidance, and verification
requirements; it does not authorize changing game settings or content merely
to reduce tool usage.
For substantial implementation delegated to Luna max, also read that skill's
model/delegation and handoff sections. Keep trivial work with the current agent.
