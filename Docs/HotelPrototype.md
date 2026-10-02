# 잔월 호텔 플레이어블 프로토타입

2026-10-03: 첫 보관함 확대 조사, 인물 포즈, 그림 수첩과 공통 퍼즐 UI·턴 연출을 추가했다. 현재 구현과 재현 방법은 [VisualInvestigation.md](VisualInvestigation.md)를 참고하세요. 아래 초기 프로토타입 설명 중 간단한 제거 전환·문자 장애물·그림 없는 수첩은 이 개편으로 대체했습니다.

Windows / Unity 6000.3.1f1. 걷기 없는 2D 조사·대화 게임에 기존 PuzzleSpire 블록 규칙을 연결한 플레이어블 버전입니다. PC 단일 화면 배경과 큰 인물, 하단 대화창으로 연출을 개편했습니다. 최신 UI 구조와 편집 방법은 [PCStagePresentation.md](PCStagePresentation.md)를 참고하세요.

## 실행

- Unity에서 `Assets/Game/Scenes/Hotel.unity`를 열고 Play.
- 또는 메뉴 `Puzzle Spire > 호텔 프로토타입 열기`.
- Windows 빌드: `Builds/Hotel/MoonlitHotel.exe`. 배포할 때 exe뿐 아니라 `Hotel` 폴더 전체가 필요합니다.
- `PuzzleTest.unity`는 기존 퍼즐 연출 실험용으로 보존했습니다. 호텔 씬은 같은 Domain 규칙과 별도의 uGUI 보드 표현을 사용합니다. 기존 BoardView의 DOTween 연출을 이식한 버전은 아닙니다.

## UI 편집: 런타임 생성 없음

`Hotel Canvas · EDIT UI HERE > Safe Area · 1600x900` 아래에 실제 UI가 저장되어 있습니다.

1. `01 Title`: 타이틀과 시작 버튼.
2. `02 Investigation`: 장소 그림, 장소 이동, 조사 버튼.
3. `03 Block puzzle`: 목표·횟수, 81개의 미리 배치된 셀, 재도전.
4. `04 Riddle`: 문제와 6개의 답변 슬롯.
5. `05 Notebook`: 12개 단서 슬롯과 설명.
6. `06 MOD and practice catalog`: 페이지당 6개 목록 슬롯.
7. `07 Settings`, `08 Ending`.
8. `09 Dialogue · actors on SAME room background`, `10 Result and confirmation overlay`, `11 Room transition · editable curtain`.

Scene 편집 중 다른 화면을 보려면 각 화면의 Active를 켜고 다른 화면을 끄세요. 플레이를 시작하면 타이틀 화면으로 정리됩니다. 배경은 `02 Investigation · SINGLE FULL-SCREEN ROOM / Room stage · entire PC viewport` 아래 Image에서 교체할 수 있습니다.

`Hotel Systems · references`의 `HotelView`에 각 위젯이 연결되어 있습니다. 오브젝트를 삭제/교체하면 여기의 참조도 연결해 주세요. `HotelGame`은 화면 표시, 대사, 목록 값만 갱신하며 Canvas/Button/Text를 생성하지 않습니다.

- 버튼 문구·패널·배치: Hierarchy/Inspector에서 편집.
- 실제 대사·문제·조사 목표: `Assets/StreamingAssets/Hotel/base.json`에서 편집. 게임 중 이 항목들은 데이터로 갱신됩니다.
- 블록 기본 이미지: `HotelGame > Default Block Sprites`의 0~4번(산호/파랑/초록/노랑/보라).
- 인물 이미지: `Default Portraits`의 서윤/하린/도식/문 선생 순서.
- 장소 기본 이미지: `HotelView > Location Images`가 가리키는 `MOD background` Image의 Sprite에 지정. MOD가 없으면 이 Sprite를 사용합니다.
- 블록 색·턴 애니메이션 속도: `HotelGame` Inspector.
- 보드 칸은 GridLayoutGroup으로 배치됩니다. 스테이지 크기에 따라 열 수·셀 크기를 조정합니다.
- 조사 버튼 위치는 노드 데이터의 정규화 좌표 `x`, `y`로 조정합니다(왼쪽 위 0,0).

`HotelSceneBuilder`는 Editor 전용 초기 저작 도구입니다. 이미 존재하는 Hotel 씬은 재생성하거나 덮어쓰지 않습니다. Play 및 빌드는 사용자가 저장한 씬을 그대로 사용합니다.

폰트는 SIL Open Font License의 나눔고딕을 프로젝트에 포함합니다. 출처는 Google Fonts의 `ofl/nanumgothic`이며 `Art/Fonts/OFL.txt`를 함께 보존합니다. 시스템에 한글 폰트를 따로 설치할 필요가 없습니다.

## 콘텐츠

- 사건 1: 사라진 303호 열쇠.
- 사건 2: 빈 객실의 전화.
- 결말: 사람을 기다리는 방.
- 조사 노드 20개, 블록 스테이지 12개, 수수께끼 4개(선택형/순서형), 단서 8개.
- 장소 3개, 주인공 1명 + NPC 3명.
- 퍼즐 연습은 모든 스테이지를 자유롭게 플레이할 수 있으며 이야기 진행을 바꾸지 않습니다.

`Tools/author_hotel_content.py`는 초기 JSON과 샘플 MOD를 만드는 원본 작성 스크립트입니다. 실행하면 그 파일들을 다시 쓰므로, JSON을 직접 수정한 뒤 무심코 실행하지 마세요. 게임이나 빌드에서는 이 스크립트를 자동 실행하지 않습니다.

스테이지는 위에서 아래로 나열한 O/X 문자열 `layout`, 왼쪽 아래가 원점인 장애물 좌표, 제한 횟수, 색 목표, 상자/얼음 목표로 구성됩니다. 블록 생성과 특수 블록 변환은 시드 기반이며 재도전마다 시드가 바뀝니다. 유효한 클릭이 없으면 추가 횟수 소모 없이 재배치됩니다.

노드 종류는 `dialogue`, `stage`, `riddle`, `ending`입니다. `requires`는 먼저 완료할 노드 ID, `rewards`는 지급할 단서 ID입니다. `before` → 플레이 → `after` → 노드 완료/자동 저장 순서로 진행합니다. 문제는 `choice` 또는 `order`, 정답은 0부터 시작하는 선택지 인덱스 배열입니다.

## 저장

설정 화면에 `Application.persistentDataPath`의 실제 경로가 표시됩니다.

```
<persistentDataPath>/
  preferences.json
  Saves/base.json
  Saves/moonlight.example.json
  Mods/
```

조사 완료와 장소 이동 때 저장합니다. 진행 중인 블록 보드는 저장하지 않으며 중단하면 해당 조사부터 다시 시작합니다. 타이틀/중단 창에도 안내합니다. 저장은 임시 파일을 쓴 뒤 교체하고 직전 파일을 `.bak`으로 보존합니다. 손상된 최근 저장은 유효한 백업이 있을 때 복구합니다.

기본 게임과 MOD의 저장은 분리됩니다. MOD 버전이 달라진 저장은 자동 변환하지 않고 오류를 표시합니다. 이전 버전의 MOD를 복구하거나 새 조사를 시작해야 합니다. 향후 정식 버전에서는 저장 마이그레이션이 필요합니다.

## MOD 제작 및 적용

타이틀 → MOD 관리 → MOD 폴더 열기 → MOD 폴더 복사 → 목록 새로고침 → 항목 선택. 선택 즉시 적용하며 한 번에 하나만 활성화합니다. `샘플 MOD 설치`로 내장 예제를 사용자 폴더에 복사할 수 있습니다. 이미 설치된 샘플은 덮어쓰지 않습니다.

```
Mods/MyMod/
  manifest.json
  content.json
  red-block.png
```

```json
{
  "schemaVersion": 1,
  "id": "my.hotel.mod",
  "name": "내 호텔 MOD",
  "version": "1.0.0",
  "author": "작성자",
  "description": "첫 스테이지와 블록 이미지 변경",
  "contentFile": "content.json",
  "images": [{ "key": "block.0", "file": "red-block.png" }]
}
```

`content.json`은 기본 데이터와 같은 형식이며 바꾸려는 배열만 넣을 수 있습니다. 동일 ID는 **항목 전체**를 교체하고 새 ID는 추가합니다(개별 필드 패치 아님). 신규 스테이지는 퍼즐 연습 목록에 자동으로 나타납니다. 이야기에서 사용하려면 `nodes`의 `contentId`로 연결해야 합니다. 기존 노드는 유지되므로 사건 흐름을 바꾸려면 기존 노드를 같은 ID로 교체하세요.

이미지 키:

- `block.0` ~ `block.4`
- `background.lobby`, `background.corridor`, `background.archive`
- `character.you`, `character.front`, `character.keeper`, `character.guest`

PNG 최대 8MB, 한 변 최대 4096px. 폴더 밖 경로, 링크 파일, 중복 ID, 잘못된 조건·목표·JSON은 거부합니다. 디코딩 불가능한 이미지는 기본 그림으로 대체됩니다. 미리보기/ZIP 자동 설치/여러 MOD 충돌 해결/새 규칙 실행은 아직 지원하지 않습니다.

현재 편집된 UI의 수용량: 장소 3개, 단서 12개, 보드 최대 9×9, 문제 선택지 최대 6개, 노드 200개, 스테이지 200개, 문제 100개. 이보다 늘리려면 UI 슬롯과 검증 규칙을 함께 확장하세요.

## 확인 방법

- Unity Test Runner → EditMode → Run All.
- Windows 빌드: `Unity.exe -batchmode -nographics -projectPath "<project>" -executeMethod PuzzleSpire.Hotel.Editor.HotelSceneBuilder.CreateAndBuild -quit -logFile "<log>"`.
- 통합 검사: `MoonlitHotel.exe -hotel-smoke -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -logFile "<log>"`.
- 통합 검사는 별도 임시 저장 폴더를 사용하고 사용자의 실제 진행을 변경하지 않습니다. 완료 후 종료하며 `Builds/Hotel/Smoke`에 화면과 결과를 기록합니다.

2026-10-02 검증: EditMode 113개 통과. Windows 개발 빌드 성공. 실행 파일 자동 진행 검사에서 조사 20개와 결말, 블록 퍼즐 12개, 수수께끼 4개, 저장/복원, MOD 이미지·스테이지, 저장 분리를 확인했습니다. 1600×900 및 1280×720 화면을 검토했으며, 작은 해상도에서는 버튼 중심 좌표의 UI raycast도 검사했습니다. 숨긴 테스트 창은 정상 화면 갱신을 생략할 수 있어 검사 때만 기존 Canvas를 카메라 방식으로 렌더링합니다. 일반 플레이는 씬의 Screen Space Overlay 설정을 사용합니다. 사람에 의한 난이도·재미 검증과 다른 PC에서의 검증은 아직 수행하지 않았습니다.

## 다음 개발 과제

현재 버전은 시스템 검증용 플레이어블입니다. 걷기·풀보이스·상점·네트워크 기능은 범위에 포함하지 않았습니다. 정식 일러스트, 배경 음악, 더 풍부한 블록 연출, 수작업 난이도 검증, 이야기 문장 편집, 키보드 전체 탐색, 저장 버전 마이그레이션은 후속 작업입니다. 기본 퍼즐의 색/특수 조합 로직은 기존 Domain을 공유하지만 호텔용 보드 애니메이션은 간단한 제거 전환입니다.
