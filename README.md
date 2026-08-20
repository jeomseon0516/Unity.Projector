# Jeomseon Unity Projector

Unity 6000.5의 `Graphics.RenderMesh`, `RenderParams`, `MaterialPropertyBlock`을 사용하는 Mesh 기반
투영 패키지입니다. 내부 Material은 Projector가 소유하며 사용자는 검증된 `ProjectorEffect`와 의미
기반 프로퍼티만 설정합니다.

첫 버전은 Orthographic Box Volume과 `MeshRenderer`, `SkinnedMeshRenderer`, `Terrain` Receiver를
지원합니다. Terrain은 설정 가능한 해상도의 투영용 메시를 내부 생성합니다. 화면 공간 Decal과
파이프라인별 조명 통합은 지원하지 않습니다.

## 수신 표면과 갱신

- `Automatically Collect Receivers`가 켜져 있으면 Projector 이동·크기·Layer 변경 직후와
  `Automatic Refresh Interval`마다 볼륨과 겹치는 활성 Receiver를 다시 찾습니다. 기본값은 0.5초이며
  0으로 설정하면 변경 직후 또는 `RefreshReceivers()`를 호출했을 때만 수집합니다.
- 정적인 큰 Scene에서는 자동 수집을 끄고 Inspector 목록 또는 `RefreshReceivers()`로 갱신하면 전체
  Scene 검색 비용을 피할 수 있습니다.
- Terrain 투영 메시(기본 129×129)는 수신자 재검색마다 만들지 않고 캐시하며, 높이맵 또는 해상도가
  바뀔 때만 다시 생성합니다.
- `SkinnedMeshRenderer`는 변형된 표면을 따라가기 위해 렌더링하는 매 프레임 `BakeMesh`를 호출합니다.
  다수의 고해상도 Skinned Mesh에는 별도 성능 검증이 필요합니다.

Projector가 만든 Material과 생성 Mesh는 컴포넌트가 소유하며 Disable/Destroy 시 해제합니다.
`ProjectorEffect`의 Shader는 `_ProjectorEffectVersion` 계약을 선언해야 합니다.
