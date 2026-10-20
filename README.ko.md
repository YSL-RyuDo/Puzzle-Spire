# Moonlit Hotel

[English](README.md) · [Korean](README.ko.md) · [Japanese](README.ja.md)

조사·대화·퍼즐을 결합하고 로컬 스토리 제작을 지원하는 미스터리 어드벤처입니다.

**C# · Unity · JSON / PNG** · 플레이 가능한 프로토타입

[개발 사례 보기](https://ysl-ryudo.github.io/ko/work/moonlit/) · [Portfolio](https://ysl-ryudo.github.io/ko/)

[Puzzle-Spire](https://github.com/YSL-RyuDo/Puzzle-Spire) (비공개 · 접근 권한 필요)

## 시스템별 담당 내용

| 영역 | 구현 |
|---|---|
| 규칙 | 퍼즐 도메인 규칙과 클리어 검증 |
| 표현 | 조사·대화·퍼즐 UI |
| 스토리 콘텐츠 | JSON·PNG 로딩, 편집·내보내기, 로컬 스토리별 저장 |

## 내 기여

- 조사·대화·퍼즐 진행을 구현하고 퍼즐 규칙과 화면 표현을 분리했습니다.
- 퍼즐 클리어 검증, 이야기 편집·내보내기와 MOD별 저장 분리를 구현했습니다.
- 로컬 JSON·PNG 콘텐츠 로딩과 테마 전환을 구현했습니다. 별도 플랫폼 백엔드는 이 클라이언트의 기여 범위에 포함하지 않습니다.

## 기여 근거

- [Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

커밋은 설명한 변경의 근거이며 기여율 점수가 아닙니다. 외부 에셋과 팀원 작업의 크레딧은 구분합니다.

## 실행·코드 확인

[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)의 Unity 버전으로 프로젝트를 여세요. 패키지 잠금 파일과 직렬화된 에셋을 유지하고, Unity를 열기 전에 문서에서 설명한 브랜치를 선택하세요.

- Unity scene: [Assets/Game/Scenes/Hotel.unity](Assets/Game/Scenes/Hotel.unity)

## 개발·검증 범위

플레이 가능한 프로토타입입니다. 기존 README의 2026-10-08 기록에는 EditMode 141개 통과가 있으며 이후 이야기 MOD 진행 기록도 있습니다. 합법적인 퍼즐 수를 이용한 자동 진행·UI 검사는 사람의 난이도·재미 평가를 대신하지 않습니다. 제작실의 플랫폼 직접 업로드는 미구현이며 외부 ZIP은 MOD 폴더에 압축 해제합니다.

## 상세 문서

- [기존 개발·사용·검증 상세 안내](docs/DEVELOPMENT_REFERENCE.ko.md)
- [Docs/FullPlayVerification.md](Docs/FullPlayVerification.md)
- [Docs/HotelPrototype.md](Docs/HotelPrototype.md)
- [Docs/ThemeMods.md](Docs/ThemeMods.md)
