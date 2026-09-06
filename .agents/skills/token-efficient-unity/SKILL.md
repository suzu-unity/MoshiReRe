---
name: token-efficient-unity
description: Run Unity Editor and Unity MCP development with low context, tool, and image cost while preserving structural, runtime, and visual QA. Use for Unity changes involving MCP calls, hierarchy, console, domain reloads, screenshots, or repeated validation.
---

# Token-efficient Unity workflow

Use this skill for the workflow around a Unity change. It does not authorize
changing game content, scenes, project settings, MCP configuration, or public
APIs merely to save tokens. The goal is equivalent or better evidence with
fewer round trips; a reduction in the Codex five-hour usage window is not
guaranteed.

## Set the acceptance bar first

Before editing, state the smallest acceptance criteria that match the task:
the expected code or structure, the runtime path and state to exercise, and,
only when appearance matters, the visual conditions to inspect. For UI, define
the target resolution, Japanese text readability, relevant existing design
reference, and the few relevant states (for example key interaction, empty,
or long-text state). A compile check or object-exists check alone cannot pass a
UI or runtime behavior task.

Classify each check as structural, runtime, or visual. Plan one coherent
implementation and verification cycle before opening Unity. Reuse evidence
only while the relevant state is unchanged.

## Discover narrowly

- Search for symbols, paths, GameObject names, and settings before reading
  files or querying a large Unity structure. Read only the relevant ranges;
  prefer `rg`, targeted reads, and the diff. Do not scan `Library`, `Temp`,
  `Logs`, `Build`, or `obj`.
- Use ordinary file operations for code, text, and settings. Use Unity MCP for
  facts or mutations that require Unity. Do not invent tool names, batch APIs,
  dynamic groups, or capabilities from another Unity skill or MCP version.
- Ask MCP for bounded output: target objects/components/properties, recent
  errors, relevant files, or a diff. Avoid full project listings, full
  hierarchy, full console, and large serialized or scene dumps.
- For generated prefabs or scenes, inspect target object names, serialized
  fields, and focused diffs instead of dumping the complete artifact. Stop
  reading once the acceptance evidence is sufficient.

### Current environment fact (2026-09-06)

This project declares `com.coplaydev.unity-mcp` from Git at `#v10.0.0`, and the
user MCP configuration points `unityMCP` at `http://127.0.0.1:8080/mcp`. In the
inspection that produced this skill, this session exposed zero Unity tools and
`tools/list` at that endpoint was refused. Batch execution and dynamic tool
groups are therefore unconfirmed. Do not change the package, endpoint, or
configuration to compensate. On a later reconnect, inspect the actually
exposed tools first and update this fact only when verified. A global Unity MCP
skill's batch examples are guidance, not evidence that this installation
supports them; the user's bounded-read policy takes precedence.

Repo-local skills are kept under `.agents/skills` and are loaded on demand; see
the [Codex build-skills documentation](https://learn.chatgpt.com/docs/build-skills)
for the supported layout.

## Group compatible work, preserve dependencies

Collect related file edits and related independent Unity mutations, then do one
refresh/compile for the coherent change. If the current MCP really exposes a
batch primitive, use it for independent operations and bounded independent
reads. Never batch an operation whose input depends on an earlier mutation,
selection, refresh, compile, domain reload, play-mode transition, or runtime
result. Such steps must be sequential, with the new state checked before the
dependent action.

If an operation already triggers import/compilation, wait for it instead of
requesting another refresh. Documentation-only work needs no Unity compile.

After any state-changing operation, refresh, compile, scene load, play-mode
transition, or domain reload, invalidate cached hierarchy data, console output,
object handles, and assumptions that may have changed. Re-query only the
targeted evidence needed for the next step. Do not repeatedly re-read unchanged
files or re-fetch unchanged hierarchy/console data.

When waiting for Unity, a busy/working state can be normal progress. Do not
treat it as failure or busy-poll; use a bounded status check and increase the
interval when the state remains busy.

## Verification order

For each coherent change, use the least expensive check that still proves the
acceptance criteria, in this order:

1. Refresh/compile once after related edits.
2. Query focused errors and exceptions, preferring recent failures over the
   entire console.
3. Query the relevant object, component, property, assignment, or hierarchy
   path using structured data.
4. Run available automated tests when they cover the changed behavior.
5. Exercise the actual runtime path in Play Mode or the equivalent execution
   path for every behavior, interaction, or scene/UI change.
6. Perform visual inspection last when visual criteria apply.

Runtime verification is required for behavior changes. If a required check was
not run, report it as skipped with the reason and leave the task unverified;
never claim completion based only on compilation, existence, or intention.

## Unity evidence policies

Use a full hierarchy query only to find an unknown target or diagnose scene
structure. Once a path or object is known, query that target and reuse the
result until a relevant scene mutation or domain reload invalidates it.

Read console output after a compile, after a runtime problem, or for final
verification. Request errors and exceptions first, then only warnings needed to
explain the issue. Do not repeatedly confirm an empty console or retrieve the
same error after it has already informed the fix.

Take a Game View or Scene View screenshot only when an image is needed to judge
layout, character/sprite display, animation, camera, color, effects, draw
order, or another visual criterion. Structural data is the right evidence for
compile state, object existence, active state, transforms, components,
serialized references, inspector values, script assignment, console errors,
and scene composition.

For a visual cycle, one sufficiently current image is a target, not a hard
limit. Never omit necessary visual QA to meet that target. Make related edits,
perform structural and runtime checks, then capture the smallest useful set of
images. If an image reveals several issues, fix them together before capturing
the next image. Reuse a fresh image while the inspected state has not changed.
Verify a shared visual component once, then inspect pages with distinct layout
or behavior changes. Use enough resolution to read the relevant text. Once
required checks pass, repeat them only for new changes, failures, or unresolved
concerns; the final report can reuse those results.

If optional MCP tool groups or dynamic loading are actually available, enable
only the groups needed for the current task and release them when finished.
Preserve pre-existing enabled groups and restore only changes made by this task.
If the current environment does not expose that control, do not simulate it or
modify configuration in its place.

## Model and delegation discipline

Respect the model selected by the parent task. When GPT-6 Astra is available,
use it for task decomposition, cross-system design, MCP planning, difficult or
unclear debugging, and the final integration review. Assign substantial,
well-scoped implementation and verification work directly to Luna max when
delegation is available. Keep trivial edits, renames, routine one-file fixes,
and simple GameObject operations with the current agent.

Pass only the objective, acceptance criteria, owned files, relevant references,
and verification commands; do not fork full history. Keep asset generation,
implementation, and Unity integration separately owned when practical; reuse
the returned artifact. Escalate ambiguity, unexpected cross-system impact,
unsafe operations, or repeated verification failures to the parent. Lower
Luna workers are optional for bounded independent work only when the handoff
cost is justified; Luna max integrates and verifies their results.

Avoid duplicate reviews and unnecessary multi-stage delegation. Use one clear owner for a
work unit and at most one focused implementation/verification handoff plus the
parent's final review when that adds evidence. Do not delegate merely to repeat
the same inspection, screenshot, hierarchy query, or console query. If model
switching or delegation is unavailable, follow the same bounded workflow in
the current model.

## Handoff and completion

Only a long interruption needs a handoff note. Keep it short: current state,
changed files, next verification, and blockers. Routine progress does not need
a transcript of every command or MCP call.

Finish with a compact report containing changed files, `git diff --stat`, tests
and Unity checks with PASS/FAIL/SKIPPED reasons, unresolved issues or risks,
and the commit hash or `未コミット` with the reason. Include capability limits
when they prevented a check. Do not mark the work complete while required
structural, runtime, or visual evidence remains unverified.
