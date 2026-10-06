# 기본 손그림과 테마 MOD

2026-10-06. 기본 그림은 작은 눈·긴 코·서로 다른 체형의 간결한 셀 일러스트 인물과 크림색·황토색·올리브색 호텔 배경으로 다시 만들었다. 첫 번째 큰 눈의 Storybook 시안은 사용하지 않는다. 기존 그림은 삭제하지 않고 `ClassicIllustrated` 외형 MOD로 보존했다. 게임 규칙, 대사, 조사 순서와 수집품 UI 배치는 그대로 유지한다.

## 플레이어 적용

시작 화면 → MOD 관리 → **클래식 일러스트 · 기존 그림** 선택 → 돌아가기 → 계속하기. 기본으로 되돌리려면 **기본 이야기**를 선택한다. 클래식 테마는 첫 실행 때 사용자 MOD 폴더에 설치된다. 이미 설치된 폴더는 사용자 수정 여부와 관계없이 덮어쓰지 않는다.

외부 MOD는 **MOD 폴더 열기**로 연 폴더 안에 `manifest.json`을 포함한 폴더를 넣고 **목록 새로고침** 후 선택한다. ZIP은 먼저 압축을 풀어야 한다. 현재는 한 번에 MOD 하나만 선택하며 테마와 콘텐츠 MOD를 동시에 조합하는 기능은 없다.

- `kind: "theme"`: 그림만 변경. 기본 이야기의 `Saves/base.json`을 함께 사용한다. 테마를 바꾸거나 해제해도 이어서 조사할 수 있다. `contentFile`을 넣으면 적용을 거절한다.
- `kind: "content"` 또는 생략: 기존 방식. 이야기·스테이지를 변경할 수 있고 MOD ID·버전별 저장 검증을 사용한다.
- 테마에서 **새 조사 시작**을 선택하면 기본 이야기의 진행을 초기화한다. 테마 자체에 별도 진행을 만들지 않는다.

배포 가능한 기존 그림: `Builds/Mods/ClassicIllustrated-1.0.0.zip`. 번들 원본: `Assets/StreamingAssets/Hotel/Themes/ClassicIllustrated/`. 실행 파일 배포는 `Builds/Hotel` 전체를 함께 전달한다.

## 제작 형식

```json
{
  "schemaVersion": 1,
  "id": "my.hotel.theme",
  "name": "나의 호텔 그림",
  "version": "1.0.0",
  "kind": "theme",
  "images": [
    { "key": "background.lobby", "file": "lobby.png" },
    { "key": "special.bomb", "file": "pieces.png", "x": 0, "y": 0, "width": 128, "height": 128 }
  ]
}
```

영역 생략 시 PNG 전체를 사용한다. 영역은 원본 PNG의 왼쪽 아래가 원점인 픽셀 좌표다. 같은 PNG를 참조하는 여러 영역은 런타임에 텍스처 하나를 공유한다. PNG당 8MB, 최대 4096×4096. 폴더 밖 경로, 연결 파일/폴더, 잘못된 영역과 중복 키는 거절한다. 이미지가 없는 키는 기본 그림을 사용한다.

| 용도 | 키 |
| --- | --- |
| 장소 | `background.lobby`, `background.corridor`, `background.archive` |
| 보관함 확대 | `inspection.cabinet` |
| 인물 | `character.you`, `character.front`, `character.keeper`, `character.guest` |
| 표정 | `character.you.curious`, `character.you.found`, `character.front.worried`, `character.front.remember` |
| 일반 블록 | `block.0`~`block.4` |
| 특수 블록 | `special.horizontal`, `special.vertical`, `special.bomb`, `special.color` |
| 장애물 | `obstacle.box`, `obstacle.box.damaged`, `obstacle.ice`, `obstacle.ice.damaged` |
| 공통 단서 물건 | `clue.thread`, `clue.key`, `clue.towel` |
| 개별 수집품 | `clue.handover`, `clue.moon_tag`, `clue.phone_log`, `clue.voice_note`, `clue.old_plan`, `clue.photo`, `clue.letter` |
| 추가 단서 기본 그림 | `clue.fallback` |

수집품은 `clue.<image>` → `clue.<id>` → 내장 그림 순서다. UI 프레임, 글꼴, 배치와 효과 애니메이션은 이번 테마 형식의 교체 대상이 아니다. 임의 코드를 실행하지 않는다.

## Unity 편집

UI는 `Assets/Game/Scenes/Hotel.unity`의 기존 Hierarchy에 저장한다. 런타임에 Canvas, Button, Text를 생성하지 않는다.

- 기본 인물·블록: `Hotel Systems · references / HotelGame`.
- 기본 특수 블록·장애물·소품·표정·보관함: `HotelInvestigationView`.
- 기본 수집품: `HotelCollectionView`.
- 배경을 중복 사용하는 씬 Image의 테마 연결: `HotelThemeView / Backgrounds`. 해당 `Default Sprite`와 Image를 함께 편집한다. MOD를 해제하면 이 기본 참조로 복귀한다.
- 기본 PNG와 Sprite: `Assets/Game/Hotel/Art/Mystery*`. 생성 도구는 내장 image_gen. [최종 프롬프트](MysteryArtPrompts.txt).

`UpgradeMystery`은 기존 그림 MOD 내보내기와 씬 참조 변경을 위한 최초 1회 도구다. 적용된 씬에서 다시 실행해도 사용자 편집을 덮어쓰지 않는다. `RefineMysterySlicesAndBuild`는 생성 아틀라스의 투명 간격을 따라 Sprite 영역을 다시 계산하는 명시적 도구다. 일반 빌드는 영역을 재계산하지 않는다.

## 검증 재현

`-hotel-theme-review`는 임시 저장에서 실제 MOD 목록 버튼으로 기본 → 클래식 → 기본을 선택하고, 동일한 저장 파일·엔딩 이어하기·대화·조사 확대·퍼즐·수집품·정적 배경을 검사한다. `Builds/Hotel/ThemeReview`에 화면을 남긴다. 전체 수집 상태는 UI 검증용으로 만든 상태이며 실제 캠페인 영상이 아니다.

추가로 EditMode에서 테마 공유 저장, 재실행과 사용자 MOD 보존, 아틀라스 영역 및 텍스처 공유, 콘텐츠 변경 차단을 검사한다. 기존 `-hotel-smoke`는 실제 캠페인 규칙으로 20개 조사 완료를 검사한다.

2026-10-06 결과: EditMode **121/121 통과**. Windows 빌드 성공. 캠페인 자동 플레이 **20개 조사·엔딩 통과**. 테마 버튼 전환과 저장 보존은 1600×900 및 1280×720에서 통과했고, 수집품 12개 페이지·미발견 숨김·상세 보기·MOD 우선순위도 1280×720에서 통과했다. 오른쪽 인물의 반전 기준점을 중앙으로 수정해 겹침을 제거했다.

`Builds/Hotel/ThemeReview/1600x900/`와 `1280x720/`에 기본/클래식 비교 화면이 있다. `Builds/Hotel/FirstInvestigation/MoonlitHotel-Mystery-FirstInvestigation.mp4`는 최종 그림으로 녹화한 **40.375초, 24fps, 무음** 첫 조사 영상이다. 실제 UI 클릭 25회, 합법적인 보드 이동 6회로 첫 퍼즐을 완료하고 단서 발견·수집품·저장 복원을 확인했다. 강제 승리는 사용하지 않았다. 전체 영상 디코딩 검사도 통과했다. 이 영상은 전체 캠페인 녹화가 아니며 기존 `FullPlay` 폴더의 영상은 이전 그림을 사용한다.

검증 원본은 `Builds/Hotel/Verification/puzzlespire-mystery-*`에 보관한다. 큰 눈의 폐기 시안은 `Builds/Backups/RejectedStorybook`에만 보관하며 현재 Unity 씬에서 참조하지 않는다. 새 기본 그림과 기존 그림 MOD는 모두 AI 생성 에셋이며, 최종 아트 취향 판단은 실제 화면 기준으로 검토할 수 있다.

## MOD를 바꾸는 실제 플레이 영상

`Builds/Hotel/ThemeSwitch/MoonlitHotel-MOD-ThemeSwitch.mp4` — **86.458초, 1600×900, 24fps, 무음**, 탐색용 챕터 포함. 첫 조사를 정상 완료한 뒤 실제 MOD 메뉴에서 클래식 → 기본을 선택한다. 각각 이어하기, 획득한 실 상세 보기, 다음 이야기 퍼즐에서 합법적인 클릭 1회, 미완료 퍼즐 나가기를 수행한다. 작은 퍼즐 인물·배경·블록·수집품이 바뀌는 모습을 확인할 수 있다.

- 0:00 기본 손그림으로 첫 조사와 퍼즐.
- 0:40 MOD 관리로 이동하여 클래식 테마 적용.
- 0:49 같은 진행을 이어서 클래식 수집품·퍼즐 확인.
- 1:02 MOD 관리로 돌아가 기본 그림 선택.
- 1:11 기본 그림의 수집품·퍼즐 확인.

검증 결과: 실제 UI 클릭 **51회**, 합법적인 보드 이동 **8회**, 첫 퍼즐 완료 **1개**, 완료 조사 **2개**. MOD 왕복 중 저장 파일 내용이 변하지 않았으며 추가 퍼즐은 완료하지 않고 나왔다. 실제 사용자 저장은 사용하지 않았다. 전체 영상 디코딩 통과. 전체 캠페인 완주 영상은 아니다.

재현 옵션: `-hotel-full-play -hotel-theme-showcase -hotel-encoder <ffmpeg 실행 파일 경로>`. 원본 영상·타임라인·검증 결과와 전환별 화면은 같은 `ThemeSwitch` 폴더에 있다. 이 옵션은 QA 자동 조작이며 일반 플레이 UI를 새로 생성하지 않는다.
