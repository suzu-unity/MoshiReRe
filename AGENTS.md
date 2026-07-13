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

# Sub-agent Workflow

For this project, Sol is the primary/orchestrating agent. When a task can be
delegated safely, Sol should assign implementation and verification work to a
cost-efficient sub-agent (for example, luna; other capable low-cost models may
be used as they become available). A more capable model may be used when the
task requires difficult debugging, broad architectural judgment, or repeated
failure recovery.

## Delegation Rules

- Sol defines the task scope, acceptance criteria, files or system in scope,
  and required tests before delegating.
- Prefer one focused sub-agent task per gameplay system or change unit.
- The sub-agent should inspect the relevant files, make the smallest safe
  change, run the appropriate tests, and avoid unrelated cleanup.
- The sub-agent should not scan or modify ignored folders: `Library`, `Temp`,
  `Logs`, `Build`, and `obj`.
- The sub-agent should preserve public APIs, serialized fields, inspector
  compatibility, and existing event flows.
- If the task is ambiguous, destructive, requires a new external authority, or
  crosses system boundaries unexpectedly, stop and escalate to Sol instead of
  guessing.

## Progress and Reporting

Sub-agents should not send routine intermediate progress updates to Sol. They
should return one compressed completion report after the work is finished. An
immediate escalation is allowed only for a blocker, failed verification that
cannot be resolved safely, an unexpected scope expansion, or a decision that
materially affects the design.

The completion report must contain only:

1. Changed files
2. `git diff --stat`
3. Test/verification results, including tests not run and why
4. Unresolved issues or risks
5. Commit hash, or `未コミット` with the reason

Use this format unless a blocker requires more detail:

```text
変更ファイル:
- path/to/file

git diff --stat:
<output>

テスト結果:
- <command or Unity verification>: PASS/FAIL/SKIPPED (<reason>)

未解決事項:
- none, or concise issue list

コミットハッシュ:
- <hash or 未コミット: reason>
```

## Git and Handoff

- Work should be isolated to the assigned task and committed when the task
  scope is complete and verification has passed.
- Never reset, discard, or overwrite unrelated user changes.
- If the working tree contains unrelated changes, report them and do not mix
  them into the task commit.
- Sol should relay the compressed report to the user rather than forwarding
  the sub-agent's internal reasoning or routine progress log.
- The commit hash is evidence of the handoff state; it does not replace test
  results or a summary of unresolved issues.

## Efficiency Defaults

- Batch independent read-only inspections or checks when possible.
- Delegate implementation and tests together so the sub-agent can verify its
  own change before handing it back.
- Avoid delegating trivial one-file inspections when the orchestration overhead
  exceeds the expected token savings.
- For larger work, split by independent system boundaries and perform a final
  integration review in Sol before reporting completion.

## Token-Efficient Workflow

The following rules are mandatory for reducing parent-agent and sub-agent token
usage while preserving implementation quality.

1. Split broad requests into explicit units such as shared infrastructure, one
   page or gameplay system, asset generation, and Unity verification. Do not
   combine unrelated implementation units in one sub-agent assignment.
2. Do not fork full conversation history into a sub-agent by default. Provide
   only the objective, acceptance criteria, owned files, required references,
   and verification commands needed for that assignment.
3. Restrict inspection to the assigned methods, files, and directly related
   dependencies. Do not read an entire builder, prefab, or subsystem when a
   targeted search and a narrow line range are sufficient.
4. Batch compatible code changes and perform Unity refresh, compilation,
   prefab rebuilding, and scene regeneration once per coherent change set
   instead of after every small edit.
5. Visually verify the shared component once and then capture only pages whose
   unique layout or behavior changed. Do not screenshot every page when the
   tested shared component is identical.
6. Sub-agents must return only the compressed completion report defined above.
   Do not return internal reasoning, copied source files, routine progress, or
   long explanations unless escalation is required.
7. Do not inspect full generated prefab or scene diffs. Verify generated output
   with targeted object names, serialized field names, `rg`, Unity hierarchy
   queries, and console results.
8. Separate asset generation, code implementation, and Unity integration into
   independent assignments when practical. The generating agent should save
   the artifact and return a path; the integrating agent should not regenerate
   the same asset.

---

End of document.
