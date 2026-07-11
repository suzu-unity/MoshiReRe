# MoshiReRe – AI Development Guide

This document describes the architecture and conventions of the MoshiReRe Unity project.
AI assistants should read this file before making code modifications.

---

# Project Overview

MoshiReRe is a Unity-based visual novel style project.

Key characteristics:

- Story-driven game using Naninovel
- Menu-driven UI systems
- Script-based gameplay logic
- Lightweight gameplay systems (money, inventory, advice triggers)

Most gameplay code is located in:

Assets/Scripts

---

# Architecture Overview

The project is structured around small independent gameplay systems.

Each system is located in its own folder.

Main systems:

MoneySystem  
MenuSystem  
LocationHUD  
Data  
LocationCard

Most systems are UI-driven and communicate via events.

---

# System Descriptions

## Money System

Folder:
Assets/Scripts/MoneySystem

Core files:

MoneyManager.cs  
MoneyUI.cs  

Responsibilities:

MoneyManager
- Singleton
- Stores current money
- Fires OnMoneyChanged event

MoneyUI
- Displays current money
- Shows animated floating change
- Plays sound effect when money changes

Pattern used:

Event-driven UI updates

---

## Menu System

Folder:
Assets/Scripts/MenuSystem

Core files:

MenuRootUI.cs  
MenuEsc.cs  
AdviceBubble.cs  
AdviceClickTrigger.cs  
AdviceTrigger.cs  
AdvicePop.cs  
MenuUIButtonHover.cs  

Responsibilities:

MenuRootUI
- Controls menu page switching

MenuEsc
- Handles ESC key toggling

Advice system
- Context help system for UI elements
- Triggered by hover or click

---

## Location System

Folders:

Assets/Scripts/LocationHUD  
Assets/Scripts/LocationCard

Responsibilities:

LocationHUD
- Displays current location

LocationCardUI
- Displays location cards in UI

---

## Data Layer

Folder:

Assets/Scripts/Data

Core files:

CharacterDatabase.cs  
CharacterInfo.cs  
InventoryDatabase.cs  
InventoryItem.cs  

Responsibilities:

CharacterDatabase
- Stores character definitions

InventoryDatabase
- Stores item definitions

InventoryItem / CharacterInfo
- Lightweight data containers

Pattern used:

ScriptableObject-like database structures

---

# Coding Conventions

AI assistants should follow these conventions.

1. Prefer small MonoBehaviour scripts
2. Avoid introducing heavy architecture frameworks
3. Prefer event-driven communication
4. UI logic should remain in UI scripts
5. Data should remain in the Data folder

---

# Important Rules for AI Code Changes

When editing code:

- Avoid breaking existing public APIs
- Do not remove serialized fields
- Maintain inspector compatibility
- Keep scripts Unity-friendly
- Avoid unnecessary refactoring

When adding features:

- Prefer adding new scripts rather than expanding existing ones too much
- Keep gameplay systems separated
- Keep UI behaviour independent

---

# AI Editing Strategy

When modifying the project:

1. Identify the gameplay system first
2. Modify the smallest possible number of files
3. Prefer extending systems rather than rewriting them
4. Preserve existing event flows

---

# Ignored Folders

AI assistants should ignore these directories:

Library  
Temp  
Logs  
Build  
obj  

These folders are not relevant to gameplay code.

---

# Entry Points

Typical gameplay entry points:

MenuRootUI  
MoneyManager  
AdviceTrigger  
LocationHUD  

These scripts often connect systems together.

---

# AI Instructions

When asked to modify gameplay:

1. Search in Assets/Scripts first
2. Identify the relevant system folder
3. Read the scripts in that folder
4. Apply minimal edits

Avoid scanning the entire repository unless necessary.

---

End of document.