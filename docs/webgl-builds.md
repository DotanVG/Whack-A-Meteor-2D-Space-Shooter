# WebGL release builds (WAM Phase 2 / M1)

Use Unity 2022.3.29f1, headless, with `-buildTarget WebGL -executeMethod WebGLBuild.Build -quit`.
Set `WAM_BUILD_LABEL` to a letters/digits/hyphens label; output is ignored under `Builds/WebGL-<label>`.
Stop the Editor that owns this project first. Never run two Editors on this project.

`McpPlayerAssemblyFilter` removes this project's generated NuGet DLLs, the MCP runtime/test-file
assemblies and its PlayerPrefsEx dependency from player build inputs. Editor assemblies and plugin
importers are untouched. No generated DLLs or define symbols need to be committed. This assumes
`Assets/Plugins/NuGet` belongs solely to MCP (as it currently does); revisit the filter if game code
starts using one of these libraries. A missing required assembly must fail the build, not be hidden.

For a controlled tooling-inclusive comparison only, set `WAM_INCLUDE_MCP=1`. The build script reads
the installed runtime asmdef's constraints and applies them through `extraScriptingDefines` to both
variants, without writing project settings. Otherwise clean committed settings can accidentally
omit MCP's runtime and understate its impact. The resolver output still requires a first Editor
import with network access; the package remains pinned to 1.5.0.

Measured from Phase 1 commit `969e2b4` with identical gameplay/assets and Brotli compression:

| Variant | Total bytes (MB) | data.br bytes | wasm.br bytes | framework.js.br bytes |
| --- | ---: | ---: | ---: | ---: |
| MCP runtime retained | 10,443,596 (10.443596) | 5,155,509 | 5,179,310 | 67,535 |
| MCP assemblies excluded | 10,445,071 (10.445071) | 5,156,968 | 5,179,310 | 67,535 |

There is no demonstrated size saving: wasm/framework are identical; the excluded variant is
1,475 bytes larger overall, predominantly serialized data/compression variation. Do not infer
browser runtime behavior from build success. The filter makes the development-only boundary
explicit; this is not a size optimization claim. Baselines exclude Phase 2 gameplay additions.
The earlier 0.63.1 package baseline was skipped to avoid another restore/build branch of the run.

Research: [Unity-MCP upstream](https://github.com/IvanMurzak/Unity-MCP) (Apache-2.0, pinned package,
active releases) is retained for Editor tooling. Unity's installed 2022.3 API documentation exposes
`IFilterBuildAssemblies`; that callback avoids mutating resolver-managed import settings or package
sources. No new third-party dependency is introduced.
