# Jeomseon Unity Projector

[English](Overview.en.md) · [설치](../README.md)

## 목적

실험적인 Orthographic Mesh 투영 도구입니다. GridTileSystem과 독립적이며 번들 Shader는 URP를 대상으로 합니다.

## 사용 흐름

ProjectorEffect와 볼륨/Receiver 설정 → RefreshReceivers 또는 자동 수집 → Graphics.RenderMesh로 표면에 투영 → Disable/Destroy 시 내부 Material·생성 Mesh 정리.

## 계약과 제한

안정화된 범용 Decal 대체재가 아닙니다. Screen-space/HDRP 통합은 없습니다. SkinnedMesh는 매 프레임 BakeMesh, Terrain은 캐시된 투영 메시를 사용하므로 대상 규모별 실측이 필요합니다.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.projector`
- Version: `0.1.1`
- Unity: `6000.6` / `0f1`
- `com.unity.render-pipelines.universal`: `17.6.0`
- `com.unity.modules.terrain`: `1.0.0`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [ProjectorBasicUsage](../Samples~/BasicUsage/ProjectorBasicUsage.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [MeshProjector](../Runtime/MeshProjector.cs)
- [ProjectorEffect](../Runtime/ProjectorEffect.cs)
- [ProjectorShaderIds](../Runtime/ProjectorShaderIds.cs)
