---
description: A description of your rule
---

Unity Development Rules

This repository contains a Unity game project.

Project Structure
	•	Gameplay scripts are located in Assets/Scripts
	•	Editor scripts are in Assets/Editor
	•	Scenes are in Assets/Scenes
	•	ScriptableObjects may be used for configuration

Coding Guidelines
	•	Use C# and Unity API conventions
	•	Prefer minimal changes to the existing architecture
	•	Avoid unnecessary refactoring
	•	Maintain readability and maintainability

Unity Best Practices
	•	Use MonoBehaviour lifecycle methods correctly
	•	Avoid expensive logic inside Update()
	•	Cache component references when possible
	•	Prefer SerializeField instead of public fields
	•	Avoid frequent FindObjectOfType calls

Performance Guidelines
	•	Avoid allocations inside Update / FixedUpdate
	•	Prefer object pooling for spawned objects
	•	Avoid LINQ inside gameplay loops

Safe Editing Rules

When modifying scripts:
	•	Do not break serialized fields used in the Inspector
	•	Preserve existing public APIs unless necessary
	•	Prefer targeted patches instead of rewriting files

Ignore Generated Unity Folders

Library/
Temp/
Logs/
Obj/
Build/
Builds/
UserSettings/

General Behavior

Always analyze the project structure before suggesting changes.
Prefer solutions consistent with Unity best practices.
Explain reasoning when proposing code changes.
