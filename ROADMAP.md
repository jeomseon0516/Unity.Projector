# Projector 로드맵

## 패키지 상태

`Jeomseon.Unity.Projector`는 **실험적(experimental) 패키지**입니다. GridTileSystem과는 의존성이나
공식 Adapter가 없는 독립 패키지이며 특수 투영 효과에만 사용합니다. 아래 P0/P1 안정화와 Sample 실측을 완료하면
기능 추가를 종료하고 동결합니다. 실제 제품 수요와 프로파일링 근거 없이 Perspective 또는
Render Pipeline별 Backend 개발을 시작하지 않습니다.

1. P0 — MeshRenderer/SkinnedMeshRenderer/Terrain Orthographic Projection과 Material 수명 안정화 (완료)
2. P1 — Receiver 캐시 무효화와 동적 Scene 지원 (완료: 주기적 수집, Terrain 높이맵 무효화,
   생성 메시 재사용)
3. P1 — 현재 미해결 Receiver 수집 및 Edit/Play Mode 컬링 회귀 해결, Sample 실측 후 안정화 종료
4. 보류 — 대규모 Terrain 부분 메시 생성과 Skinned Mesh Bake 빈도 정책
5. 보류 — Perspective Projection
6. 보류 — 선택적 Render Pipeline Backend
