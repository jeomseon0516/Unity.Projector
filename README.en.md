# Jeomseon Unity Projector

[한국어](README.md) | English

An experimental orthographic mesh projection tool, independent of GridTileSystem, with bundled shaders targeting URP.

## Quick start

Configure ProjectorEffect/volume/Receivers, RefreshReceivers or collect automatically, project via Graphics.RenderMesh, and release internal Materials/generated Meshes on Disable/Destroy.

## Contracts and limitations

Not a stabilized general-purpose Decal replacement. No screen-space/HDRP integration. SkinnedMesh uses per-frame BakeMesh; Terrain uses cached projection geometry. Profile target scene sizes.

## Requirements

- Unity `6000.6.0f1`
- Package: `com.jeomseon.unity.projector` `0.1.1`
- `com.unity.render-pipelines.universal`: `17.6.0`
- `com.unity.modules.terrain`: `1.0.0`

## Installation

Merge this into your project's `Packages/manifest.json`; preserve existing dependencies and registries.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.projector": "0.1.1"
  }
}
```

Or use **Install package from git URL** in Package Manager:

```text
https://github.com/jeomseon0516/Unity.Projector.git#v0.1.1
```

## API usage and details

### Receivers and refresh policy

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

## Samples

Import the desired entry from **Package Manager > Samples**. Use the included Scene/Editor tool and complete its Setup step first, if required.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [ProjectorBasicUsage](Samples~/BasicUsage/ProjectorBasicUsage.unity)

## Tests

[Test sources](Tests)

Add the package ID to manifest `testables`, then execute its relevant EditMode/PlayMode assemblies in Unity Test Runner.

## Further reading

[Complete source map and package metadata](Documentation~/Overview.en.md)
- [Changelog](CHANGELOG.md)

## License

[License](LICENSE.md)
