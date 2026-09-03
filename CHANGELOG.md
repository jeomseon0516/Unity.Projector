# 변경 기록

## [Unreleased]

- **(렌더 파이프라인)** 워크스페이스 Unity `6000.6` + URP `17.6` 전환. 번들 셰이더
  `Hidden/Jeomseon/Projector/Mesh Projection`과 Sample 셰이더 `Jeomseon/Projector/Sample Surface`를
  URP 전용으로 이전했습니다: `UnityCG.cginc` → URP `Core.hlsl`, `UnityObjectToClipPos` →
  `TransformObjectToHClip`, `unity_ObjectToWorld`/`UNITY_MATRIX_VP` → `TransformObjectToWorld`/
  `TransformWorldToHClip`, `sampler2D`/`tex2D` → `TEXTURE2D`/`SAMPLER`/`SAMPLE_TEXTURE2D`,
  SubShader `"RenderPipeline"="UniversalPipeline"` + Pass `"LightMode"="UniversalForward"` 태그 추가
  (기존 Mesh Projection pass에는 LightMode가 없어 URP에서 렌더되지 않았음).
- `com.unity.render-pipelines.universal` `17.6.0` 의존성을 추가했습니다. `MeshProjector`의
  `Graphics.RenderMesh`/`RenderParams`/`MaterialPropertyBlock` 경로는 파이프라인 비종속이라 C#
  변경은 없습니다.
- Unity 6000.6에서 폐기된 `FindObjectsSortMode` 인자 오버로드를 무정렬 기본
  `FindObjectsByType<T>(FindObjectsInactive)` 오버로드로 교체했습니다.
- `MeshProjector`, `ProjectorEffect` 및 기본 Texture Projection Shader를 추가했습니다.
- 투영 볼륨과 교차하는 `MeshRenderer`, `SkinnedMeshRenderer`, `Terrain` receiver를 지원합니다.
- 내부 Material을 공개하지 않고 `_ProjectorEffectVersion` 계약을 만족하는 Effect만 허용합니다.
- 자동 Receiver 수집 주기를 제공하고, 재검색 시 Terrain/Skinned 생성 Mesh 캐시를 보존합니다.
- Terrain 높이맵 변경 시에만 Terrain 투영 Mesh를 무효화하며, 회전된 Projector/Terrain Bounds를
  월드 AABB로 정확히 계산합니다.
- Unity CLI EditMode receiver/행렬/Material 비노출/캐시/회전 볼륨 테스트 5개를 추가했습니다.
