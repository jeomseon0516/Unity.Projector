# Jeomseon Unity Projector

한국어 | [English](README.en.md)

실험적인 Orthographic Mesh 투영 도구입니다. GridTileSystem과 독립적이며 번들 Shader는 URP를 대상으로 합니다.

## 빠르게 시작하기

ProjectorEffect와 볼륨/Receiver 설정 → RefreshReceivers 또는 자동 수집 → Graphics.RenderMesh로 표면에 투영 → Disable/Destroy 시 내부 Material·생성 Mesh 정리.

## 계약과 주의사항

안정화된 범용 Decal 대체재가 아닙니다. Screen-space/HDRP 통합은 없습니다. SkinnedMesh는 매 프레임 BakeMesh, Terrain은 캐시된 투영 메시를 사용하므로 대상 규모별 실측이 필요합니다.

## 요구 사항

- Unity `6000.6.0f1`
- 패키지: `com.jeomseon.unity.projector` `0.1.1`
- `com.unity.render-pipelines.universal`: `17.6.0`
- `com.unity.modules.terrain`: `1.0.0`

## 설치

프로젝트 `Packages/manifest.json`의 기존 dependencies와 registry를 유지하면서 다음 항목을 병합합니다.

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

또는 Package Manager의 **Install package from git URL**을 사용합니다:

```text
https://github.com/jeomseon0516/Unity.Projector.git#v0.1.1
```

## API 사용과 상세 설명

### 수신 표면과 갱신

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

## 샘플 실행

**Package Manager > Samples**에서 예제를 Import합니다. 포함된 Scene 또는 Editor 도구를 열고 필요한 Setup 메뉴를 먼저 실행합니다.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [ProjectorBasicUsage](Samples~/BasicUsage/ProjectorBasicUsage.unity)

## 테스트

[테스트 코드](Tests)

manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 EditMode/PlayMode 어셈블리를 실행합니다.

## 추가 문서

[전체 코드 지도와 패키지 구성](Documentation~/Overview.md)
- [Changelog](CHANGELOG.md)

## 라이선스

[License](LICENSE.md)
