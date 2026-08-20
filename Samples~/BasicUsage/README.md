# Basic Usage

`ProjectorBasicUsage` Scene은 `MeshProjector`가 내부 Material을 노출하지 않고 Layer 3의 메시 표면에
기본 Effect를 투영하는 최소 예제입니다. Built-in과 URP에서 추가 Renderer Feature 없이 열 수 있습니다.

Game View에서 어두운 Plane 중앙에 밝은 투영 영역이 보이는지 확인합니다. `Receiver Mask`를 바꾸거나
Plane을 투영 볼륨 밖으로 이동한 뒤 `Refresh Receivers`를 호출하면 대상에서 제외되어야 합니다.

이 최소 Scene은 정적 `MeshRenderer` 경로를 보여줍니다. `Terrain`과 `SkinnedMeshRenderer`도 같은
Receiver 계약으로 지원하지만, Terrain 생성 메시 해상도와 Skinned Mesh의 프레임별 Bake 비용은
프로젝트 규모에 맞춰 별도로 측정해야 합니다.
