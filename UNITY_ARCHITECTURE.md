# MoshiReRe – Unity Architecture

This document explains how the Unity side of the MoshiReRe project works.

AI assistants should read this before modifying gameplay scripts.

---

# Game Type

Narrative / Visual Novel style game.

The project combines:

- Naninovel scenario scripting
- Unity UI gameplay systems
- Small gameplay mechanics (money, items, advice system)

Most gameplay interactions are UI driven.

---

# Main Systems

## Scenario System

Handled by:

Naninovel

Scenario files:

Assets/Scenario

Naninovel controls:

- Dialogue flow
- Character appearance
- Scene transitions
- Scripted events

Unity scripts respond to these events.

---

# Gameplay Systems

Gameplay logic is located in:

Assets/Scripts

Subsystems include:

MoneySystem  
MenuSystem  
LocationHUD  
LocationCard  
Data  

Each subsystem is isolated in its own folder.

---

# System Communication

The project primarily uses:

Event-driven architecture.

Example:

MoneyManager
→ fires OnMoneyChanged event  
→ MoneyUI listens and updates display

Pattern:

Manager → Event → UI

---

# UI Structure

The UI is built using:

Unity Canvas  
TextMeshPro  
TextAnimator (Febucci)

UI systems are responsible for:

- displaying data
- animations
- sound effects

They do not contain gameplay logic.

---

# Data Structure

Game data is stored in simple container classes.

Examples:

CharacterInfo  
InventoryItem

Databases store lists of these objects:

CharacterDatabase  
InventoryDatabase

These act as lightweight data registries.

---

# Scene Logic

Scenes generally contain:

UI Canvas  
Managers (MoneyManager etc)  
Naninovel runtime

Scenes should remain lightweight.

Gameplay logic belongs in scripts, not scenes.

---

# Audio System

Audio is located in:

Assets/Audio

Subfolders:

BGM  
SFX  

Audio playback is handled by Unity AudioSource components.

AI assistants do not need to inspect audio files.

---

# Important Patterns

Singleton Managers

Example:

MoneyManager

Rules:

- only one instance
- survives scene changes
- accessible globally

Event-based updates

Example:

OnMoneyChanged

UI reacts to events rather than polling state.

---

# Unity Coding Rules

When editing scripts:

Do not break serialized fields.

Avoid renaming public fields used in the Inspector.

Avoid removing MonoBehaviour lifecycle methods.

Prefer simple MonoBehaviour scripts over complex frameworks.

---

# Safe AI Editing Guidelines

When adding features:

- Modify as few files as possible
- Prefer extending existing systems
- Keep UI logic separate from data logic
- Maintain event-based architecture

---

# Files AI Should Ignore

AI assistants should ignore:

Library  
Temp  
Logs  
Build  
*.meta  

These are generated files.

---

# Typical Feature Workflow

When implementing a feature:

1. Identify relevant subsystem
2. Read scripts in that folder
3. Identify manager or controller class
4. Add minimal logic
5. Update UI if needed

---

# Typical Entry Points

MoneyManager  
MenuRootUI  
AdviceTrigger  
LocationHUD  

These scripts often coordinate other systems.

---

End of document