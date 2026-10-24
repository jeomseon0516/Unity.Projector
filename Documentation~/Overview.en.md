# Jeomseon Unity Projector

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

An experimental orthographic mesh projection tool, independent of GridTileSystem, with bundled shaders targeting URP.

## Usage flow

Configure ProjectorEffect/volume/Receivers, RefreshReceivers or collect automatically, project via Graphics.RenderMesh, and release internal Materials/generated Meshes on Disable/Destroy.

## Contracts and limitations

Not a stabilized general-purpose Decal replacement. No screen-space/HDRP integration. SkinnedMesh uses per-frame BakeMesh; Terrain uses cached projection geometry. Profile target scene sizes.

## Package requirements

- Package ID: `com.jeomseon.unity.projector`
- Version: `0.1.1`
- Unity: `6000.6` / `0f1`
- `com.unity.render-pipelines.universal`: `17.6.0`
- `com.unity.modules.terrain`: `1.0.0`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [ProjectorBasicUsage](../Samples~/BasicUsage/ProjectorBasicUsage.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

- [MeshProjector](../Runtime/MeshProjector.cs)
- [ProjectorEffect](../Runtime/ProjectorEffect.cs)
- [ProjectorShaderIds](../Runtime/ProjectorShaderIds.cs)
