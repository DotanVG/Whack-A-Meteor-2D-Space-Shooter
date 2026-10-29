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
  and fails on syntax errors and language-version diagnostics `CS8773`, `CS8703`,
  `CS8652` and `CS8967`, with file/line diagnostics. Binding uses the checker's
  .NET 8 runtime `System.Private.CoreLib` and `System.Runtime` metadata so features
  such as unsigned right shift, params collections, span constant patterns, index
  initializers and interpolated string handlers are checked too. Other compilation
  diagnostics are ignored; no Unity assemblies or license are needed.
- Unity-ignored path components (dotfiles such as `.gitkeep` and `.DS_Store`,
  names ending in `~` or `.tmp`, and `cvs`) and their descendants are excluded from
  C# and GUID checks and do not need their own metas. All tracked paths under
  `Assets`, including ignored contents, establish that their parent folders exist.
- Every tracked non-ignored, non-meta asset and non-ignored inferred parent folder
  below `Assets` must have a tracked `.meta`. The root `Assets` directory itself
  does not need `Assets.meta`.
- A non-ignored `.meta` under Assets must have a tracked asset or inferred folder counterpart.
  A `.gitkeep`-only folder or a folder containing only `Samples~` contents is valid
  with its folder `.meta`. Empty folders otherwise have no Git representation.
- Every tracked non-ignored `.meta` in the repository must have exactly one 32-hex GUID, with
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

This gate is necessary but not sufficient: its C# checks cover syntax and language
version only. The BCL references enable feature detection, not a Unity compile.
It does not catch unresolved types or names (for example `CS0246`/`CS0103`), the
C# 14 `field` keyword (parsed as an ordinary identifier under C# 9), missing
`IsExternalInit`/`CS0518` for `init` accessors and records, or unsupported covariant
returns. Passing this gate does not prove Unity compiler or runtime compatibility.
Unity import, EditMode/PlayMode tests, gameplay validation and builds are separate.
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
