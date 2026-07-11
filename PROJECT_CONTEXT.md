# MoshiReRe – Project Context

This file provides high-level context about the MoshiReRe Unity project so that AI assistants
(Aider, Claude, Continue, etc.) can understand the architecture quickly without scanning the entire repository.

---

# Project Type

Unity Visual Novel / Narrative Game

Key characteristics:

- Built with Unity
- Uses Naninovel for scenario scripting
- UI-driven gameplay
- Event-based systems
- Lightweight gameplay mechanics

Primary gameplay logic exists in:

Assets/Scripts

---

# Important Folders

Assets/Scripts

Contains all gameplay logic and UI logic.

Subsystems include:

MoneySystem  
MenuSystem  
LocationHUD  
LocationCard  
Data  

---

# Core Gameplay Systems

## Money System

Folder:
Assets/Scripts/MoneySystem

Files:

MoneyManager.cs  
MoneyUI.cs  

Responsibilities:

MoneyManager
- Singleton pattern
- Stores player currency
- Fires `OnMoneyChanged` event

MoneyUI
- Displays current money
- Shows animated floating text for gain/loss
- Plays sound effects

Pattern used:

Event-driven UI updates.

---

## Menu System

Folder:

Assets/Scripts/MenuSystem

Key files:

MenuRootUI.cs  
MenuEsc.cs  
AdviceBubble.cs  
AdviceTrigger.cs  
AdviceClickTrigger.cs  
AdvicePop.cs  
MenuUIButtonHover.cs  

Responsibilities:

MenuRootUI
Controls navigation between menu pages.

MenuEsc
Handles ESC key menu toggle.

Advice system
Contextual help tooltips triggered by UI hover or click.

MenuUIButtonHover
Adds hover animation effects.

---

## Location System

Folders:

Assets/Scripts/LocationHUD  
Assets/Scripts/LocationCard  

Responsibilities:

LocationHUD.cs
Displays the current location UI.

LocationCardUI.cs
Displays selectable location cards.

---

## Data Layer

Folder:

Assets/Scripts/Data

Key files:

CharacterDatabase.cs  
CharacterInfo.cs  
InventoryDatabase.cs  
InventoryItem.cs  

Responsibilities:

CharacterDatabase
Stores character definitions.

InventoryDatabase
Stores item definitions.

CharacterInfo / InventoryItem
Simple data containers.

Pattern used:

Lightweight database-style data storage.

---

# Naninovel Integration

Scenario scripts are located in:

Assets/Scenario

Naninovel handles:

- dialogue
- scene transitions
- scripted events

Gameplay systems interact with Naninovel primarily through UI or event triggers.

---

# Audio System

Audio assets are stored in:

Assets/Audio

Includes:

BGM  
SFX  

These assets are referenced by UI scripts and Naninovel scripts.

AI assistants do not need to analyze audio assets.

---

# Coding Style

The project follows these conventions:

- Small MonoBehaviour scripts
- Minimal architectural overhead
- Event-driven UI updates
- Data stored separately from UI
- Systems grouped by folder

Avoid introducing heavy frameworks or dependency injection.

---

# AI Guidelines

When modifying this project:

1. Search inside `Assets/Scripts` first
2. Identify the relevant system folder
3. Read scripts within that subsystem
4. Make minimal edits

Avoid scanning or modifying:

Library  
Temp  
Logs  
Build  
*.meta files  

These files are not relevant to gameplay logic.

---

# Entry Points

Common gameplay entry points include:

MenuRootUI  
MoneyManager  
AdviceTrigger  
LocationHUD  

These scripts often coordinate between systems.

---

# Editing Strategy

When implementing a new feature:

1. Determine which subsystem it belongs to
2. Prefer adding a new script rather than expanding existing ones
3. Preserve existing event flows
4. Avoid breaking serialized Unity fields

---

End of context.