# License-free checks

From the repository root (Windows PowerShell 5.1 or PowerShell 7, Git and .NET SDK 8+):

```powershell
./tools/check-license-free.ps1
./tools/test-license-free.ps1
```

The first command is exactly what CI runs. The second exercises a disposable copy
of the tracked Assets in an ignored nested Git repository, with deliberate failures.
Stage new/deleted assets before checking locally: Git's index defines tracked files;
the checker reads their working-tree contents. Untracked assets are not checked.

- Roslyn parses every imported, tracked `Assets/**/*.cs` as C# 9 (Unity 2022.3)
  and fails on syntax errors and compilation diagnostic `CS8773` (features newer
  than C# 9), with file/line diagnostics. Other compilation diagnostics are ignored;
  no Unity assemblies or license are needed.
- Unity-ignored path components (dotfiles such as `.gitkeep` and `.DS_Store`,
  names ending in `~` or `.tmp`, and `cvs`) and their descendants are excluded.
- Every tracked non-meta asset and inferred parent folder below `Assets` must have
  a tracked `.meta`. The root `Assets` directory itself does not need `Assets.meta`.
- A `.meta` under Assets must have a tracked asset or inferred folder counterpart.
  Empty folders have no Git representation; remove unused folder metas or add real
  tracked contents together with their metas.
- Every tracked `.meta` in the repository must have exactly one 32-hex GUID, with
  no duplicates (case insensitive).

The workflow runs on PRs targeting `dev` or `main`, with read-only contents
permission, SHA-pinned official actions, no persisted checkout credentials, and a
three-minute job timeout. CI also runs the disposable-copy regression tests.
Runs on the same workflow/ref cancel stale runs; the SDK is pinned to `8.0.427`.
The harness removes stale `.test-repo` copies and cleans up on success or failure,
including failures while creating the fixture. Linked fixture paths are rejected.
NuGet dependencies are pinned by `packages.lock.json`
and restored in locked mode. No repository secrets or Unity installation are used.

## Limits

This is a syntax/metadata gate, not compilation, Unity import, EditMode/PlayMode
tests, gameplay validation, or a build. It cannot detect type/API/reference errors.
Preprocessor symbols are unset, so inactive `#if` branches are not parsed. Syntax
warnings do not fail the check. Scene/prefab YAML and script-reference GUIDs are
not validated (package/built-in GUID resolution needs separate handling). Hosted
runner queue time and dependency downloads can vary; the timeout bounds the job,
not time waiting for a runner.

## Approach

The initial `dev` audit found a missing `GameConstants.cs.meta` and an unused
`Assets/Scripts/UI.meta` without tracked contents or GUID references. This change
adds the former and removes the latter so the strict check passes without an
exception list. Gameplay source, scenes, prefabs, and visual assets are unchanged.

Reviewed [DeNA/unity-meta-check](https://github.com/DeNA/unity-meta-check) (MIT,
maintained) for its Git-based missing/dangling-meta approach and
[Roslyn](https://github.com/dotnet/roslyn) (MIT, maintained) for C# parsing. Roslyn
is used directly; the small metadata pass shares the same .NET tool rather than
adding a second downloaded executable. No third-party action or Unity code runs.
