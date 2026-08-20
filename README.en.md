# Jeomseon Unity Projector

A mesh projection package built on Unity 6000.5 `Graphics.RenderMesh`, `RenderParams`, and
`MaterialPropertyBlock`. The Projector owns its internal Material and only accepts validated
`ProjectorEffect` assets and semantic property values.

The first version supports an orthographic box volume and `MeshRenderer`, `SkinnedMeshRenderer`, and
`Terrain` receivers. Terrain uses an internally generated projection mesh at a configurable
resolution. Screen-space decals and pipeline-specific lighting integration are outside the scope.

## Receivers and refresh policy

- With `Automatically Collect Receivers` enabled, the projector scans for active overlapping
  receivers after projector volume/layer changes and at each `Automatic Refresh Interval`. The
  default is 0.5 seconds. A value of zero limits collection to explicit changes or
  `RefreshReceivers()` calls.
- For large static scenes, disable automatic collection and use the Inspector lists or call
  `RefreshReceivers()` to avoid repeated scene-wide searches.
- Generated terrain meshes (129×129 by default) survive receiver scans and are rebuilt only after a
  heightmap or resolution change.
- `SkinnedMeshRenderer` receivers use `BakeMesh` every rendered frame to follow deformation. Profile
  scenes containing many high-resolution skinned meshes.

The component owns and releases its generated Materials and Meshes on disable/destruction. A
`ProjectorEffect` shader must declare the `_ProjectorEffectVersion` contract.
