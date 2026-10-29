# Phase 2: authored encounter data and shared skill upgrades

See [the gameplay design](../phase2-design.md). Runtime encounter components consume
authored data without changing it. Phase 1 owns purchases, migration and modifiers;
new gameplay reads that same API. Build tooling is isolated in an Editor folder and
filters development assemblies at the player-build boundary (see ../webgl-builds.md).
Retain existing prefabs and tags to preserve serialized references and CC0 provenance.
