# Exploration prototype source assets

These PNG files are exact, pixel-unmodified copies of the user-provided source
images. They are intentionally kept as full source images; crop rectangles are
provided for Unity Sprite Editor slicing.

## Files

| File | Source dimensions | Suggested import |
| --- | ---: | --- |
| `exploration_room_background.png` | 1916 x 821 | Sprite (2D and UI), Single, PPU 100 |
| `player_suit_strip.png` | 1448 x 1086 | Sprite (2D and UI), Multiple, PPU 100 |
| `npc_strip.png` | 1672 x 941 | Sprite (2D and UI), Multiple, PPU 100 |
| `player_casual_strip.png` | 1448 x 1086 | Sprite (2D and UI), Multiple, PPU 100 |

## Sprite crop rectangles

All rectangles use Unity `RectInt(x, y, width, height)` coordinates, where
`(0, 0)` is the **bottom-left** of the source texture. The five figures are
ordered left to right. Each crop includes a 4 px safety margin around the
detected figure and its feet/shadow while avoiding unnecessary beige backdrop.

### player_suit_strip.png

```text
PlayerSuit_01 = RectInt(55,   232, 192, 715)
PlayerSuit_02 = RectInt(338,  222, 238, 710)
PlayerSuit_03 = RectInt(620,  216, 245, 705)
PlayerSuit_04 = RectInt(909,  216, 235, 700)
PlayerSuit_05 = RectInt(1174, 214, 215, 697)
```

### npc_strip.png

```text
Npc_01 = RectInt(77,   159, 185, 694)
Npc_02 = RectInt(392,  150, 235, 692)
Npc_03 = RectInt(684,  139, 271, 697)
Npc_04 = RectInt(1019, 142, 242, 691)
Npc_05 = RectInt(1341, 137, 276, 687)
```

### player_casual_strip.png

```text
PlayerCasual_01 = RectInt(42,   245, 191, 704)
PlayerCasual_02 = RectInt(319,  231, 238, 706)
PlayerCasual_03 = RectInt(603,  226, 239, 701)
PlayerCasual_04 = RectInt(901,  223, 233, 697)
PlayerCasual_05 = RectInt(1169, 219, 225, 698)
```

## Prototype scale

Start with PPU 100 for all textures. The room background then spans 19.16 x
8.21 Unity units. A character crop is roughly 7 units high at scale 1; use a
transform scale of 0.75 (adjust within 0.70--0.80 to suit staging) for a
roughly 5.2-unit-tall on-screen character.

The images are opaque RGB artwork. To keep pixels unmodified, use the above
rectangles as the source Sprite Editor slice bounds rather than trying to
remove the beige background.
