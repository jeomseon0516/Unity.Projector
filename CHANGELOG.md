# 변경 기록

## [Unreleased]

- `MeshProjector`, `ProjectorEffect` 및 기본 Texture Projection Shader를 추가했습니다.
- 투영 볼륨과 교차하는 `MeshRenderer`, `SkinnedMeshRenderer`, `Terrain` receiver를 지원합니다.
- 내부 Material을 공개하지 않고 `_ProjectorEffectVersion` 계약을 만족하는 Effect만 허용합니다.
- 자동 Receiver 수집 주기를 제공하고, 재검색 시 Terrain/Skinned 생성 Mesh 캐시를 보존합니다.
- Terrain 높이맵 변경 시에만 Terrain 투영 Mesh를 무효화하며, 회전된 Projector/Terrain Bounds를
  월드 AABB로 정확히 계산합니다.
- Unity CLI EditMode receiver/행렬/Material 비노출/캐시/회전 볼륨 테스트 5개를 추가했습니다.
