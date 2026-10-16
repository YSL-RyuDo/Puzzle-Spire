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
| 남성 대화 자세 | `character.keeper.think`, `character.keeper.explain`, `character.guest.remember`, `character.guest.warm` |
| 일반 블록 | `block.0`~`block.4` |
| 특수 블록 | `special.horizontal`, `special.vertical`, `special.bomb`, `special.color` |
| 장애물 | `obstacle.box`, `obstacle.box.damaged`, `obstacle.ice`, `obstacle.ice.damaged` |
| 공통 단서 물건 | `clue.thread`, `clue.key`, `clue.towel` |
| 개별 수집품 | `clue.handover`, `clue.moon_tag`, `clue.phone_log`, `clue.voice_note`, `clue.old_plan`, `clue.photo`, `clue.letter` |
| 추가 단서 기본 그림 | `clue.fallback` |

수집품은 `clue.<image>` → `clue.<id>` → 내장 그림 순서다. UI 프레임, 글꼴, 배치와 효과 애니메이션은 이번 테마 형식의 교체 대상이 아니다. 임의 코드를 실행하지 않는다.

이미지 항목의 선택 필드 `flipInDialogue: true`는 그 캐릭터 그림만 대화에서 좌우 반전한다. 클래식 서윤의 발견 자세처럼 원본이 대화 상대와 반대 방향을 바라보는 경우에 사용한다. 글자와 UI에는 영향을 주지 않는다.

## 대화 자세 추가 및 분리 배포 · 1.1.0

도식의 생각하기/설명하기, 문 선생의 회상하기/따뜻하게 웃기 그림을 기본/클래식 각각 4장씩 추가했다. 여성 인물의 기존 자세도 포함해 51줄 대사 전체에 `pose`를 지정했다. 네 인물 모두 기본 그림 1종 + 대화 자세 2종을 사용한다. 요청을 잘못 이해해 추가했던 상체 흔들기·기울이기 효과는 제거했다. 기존 등장·호흡·말하는 사람 강조는 유지한다. [에셋 제작 프롬프트](DialoguePoseArtPrompts.md).

최신 배포 ZIP은 `Builds/Distribution/MoonlitHotel-Windows.zip`과 `MoonlitHotel-ClassicIllustrated-MOD-1.1.0.zip`이다. 게임은 Release 빌드의 실행 파일·런타임·게임 데이터·필수 라이선스 고지만 포함하며, 녹화·스크린샷·검증 자료·소스·디버그 심벌·번들 MOD는 제외한다. MOD ZIP은 `ClassicIllustrated/manifest.json`, 참조 이미지, 설치 설명만 포함한다. 새 MOD에는 이미지 키 40개가 있다.

게임 ZIP을 풀고 `MoonlitHotel.exe`를 실행한다. MOD ZIP을 풀어 나온 `ClassicIllustrated` 폴더를 게임의 **MOD 관리 → MOD 폴더 열기**로 연 폴더에 넣고 **목록 새로고침 → 클래식 일러스트 선택 → 타이틀 → 이어하기**를 누른다. ZIP 자체를 Mods 폴더에 넣는 방식은 지원하지 않는다. 기본 이야기의 저장을 공유한다. 기존 동일 MOD는 사용자 수정본을 백업한 뒤 교체한다.

재포장은 `Tools/package_hotel_release.py`로 수행한다. 런타임 허용 목록으로 압축하고 두 ZIP의 CRC, 금지 파일·폴더 부재, 40개 MOD 이미지 참조를 검사한 뒤 임시 폴더에 실제 압축을 풀어 실행 검증 경로를 만든다. 검증 기록은 ZIP 외부의 `Builds/Distribution/package-verification.json`과 `Builds/Hotel/Verification`에 저장한다.

최종 압축본 실행 검증 통과: MOD 없이 첫 조사 완료 및 저장 복원, 별도 MOD ZIP 수동 설치 후 모든 자세 렌더링, 기본 상태에서 시작해 MOD 선택 후 **20개 조사·12개 퍼즐·4개 수수께끼·51줄 대사·엔딩 및 엔딩 이어하기** 완료. ZIP CRC와 SHA-256 기록을 남겼다. 검증 영상과 화면은 배포 ZIP 밖의 `Builds/Hotel/Verification/PoseAssets-Release`에만 보관한다.

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

## 게임 내 퍼즐 제작실 (2026-10-08)

타이틀의 **퍼즐 제작실**에서 기본 이야기의 12개 퍼즐 중 하나를 선택해 수정한다. 초기 버전은 한 번 내보낼 때 선택한 퍼즐 하나를 교체하는 콘텐츠 MOD를 만든다. 대사·그림·UI 배치 편집기는 이 제작실에 아직 포함하지 않는다.

- 격자 크기 3~9칸, 빈 공간, 상자·얼음과 내구도 1~3, 이동 횟수, 색 종류, 목표, 시드 편집.
- 초안 저장·불러오기와 편집 되돌리기/다시 실행. 초안은 실행 가능한 퍼즐 검증을 통과하기 전에도 저장할 수 있다.
- 시험 플레이는 인증하지 않는다. 클리어 검증 모드에서 처음부터 실제 규칙으로 목표를 달성해야 내보내기 버튼이 열린다. 플레이 중 설정 변경은 잠긴다.
- 규칙·배치·목표·이동 횟수·시드가 바뀌면 인증을 무효화한다. 이름 변경은 유지한다.
- `clearance.json`에 규칙 버전, 퍼즐 지문, 합법적인 클릭 기록을 저장한다. 내보내기와 해당 MOD 로드 시 실제 퍼즐 로직으로 재생해 클리어를 검증한다. 기존 수작업 MOD의 호환성은 유지한다.
- 인증 기록은 풀 수 있다는 근거이며 플레이어가 따라야 할 정답 순서가 아니다. 다른 순서로 목표를 달성해도 정상 클리어한다.
- 제작실에서 내보낸 퍼즐은 `fixedSeed: true`로 재도전에도 동일한 시작 조건을 사용한다. 기존 기본 퍼즐은 기존 재도전 시드 동작을 유지한다.
- 내보내기는 로컬 MOD 목록 등록과 공유용 ZIP 생성을 수행한다. 온라인 플랫폼 업로드 기능은 아니다. MOD는 별도 저장을 사용하며 제작과 검증은 본편 저장을 변경하지 않는다.

공유 ZIP에는 `workshop.<고유ID>/manifest.json`, `content.json`, `clearance.json`, `README.txt`가 들어간다. ZIP을 풀고 그 MOD 폴더를 다른 게임의 MOD 폴더에 넣으면 된다. 초안은 사용자 데이터의 `Workshop/Drafts`, 공유 ZIP은 `Workshop/Exports`에 저장된다. 제작실의 **내보낸 폴더**로 열 수 있다.

UI는 `Assets/Game/Hotel/Prefabs/PuzzleWorkshop.prefab`에 저장되어 있으며 `Assets/Game/Scenes/Hotel.unity`의 `11 Puzzle Workshop · EDIT THIS PREFAB` 인스턴스에서 편집할 수 있다. 런타임에서는 UI를 생성하지 않고 저장된 필드·버튼·81개 셀을 연결한다. 초기 연결 메뉴는 `Puzzle Spire/퍼즐 제작실 추가 · 기존 UI 보존`이며 이미 연결되어 있으면 다시 생성하지 않는다.

검증 결과: EditMode **135/135 통과**. 실제 Windows UI에서 제작→시험→검증→인증 무효화→재검증→ZIP 내보내기→MOD 선택→일반 퍼즐 클리어 통과. 기존 본편의 20개 조사·12개 퍼즐·4개 수수께끼·엔딩 회귀 검사도 통과했다.

검증 파일: `Builds/Hotel/Verification/workshop-tests.xml`, 실제 Windows UI 검사 `Builds/Hotel/WorkshopReview/result.txt`와 01~09 화면. QA는 별도의 임시 사용자 데이터에서 합법적인 자동 클릭을 사용하며 본인의 수동 제작 클리어 기록으로 오해하지 않도록 샘플 ZIP을 `Verified-Puzzle-Sample.zip`으로 구분한다. 재현 옵션은 `-hotel-workshop-review -screen-width 1600 -screen-height 900 -screen-fullscreen 0`이다.

## 이야기 제작실: 새 챕터와 독립 이야기 만들기 (2026-10-08)

타이틀의 **이야기 제작실**이 대사·조사·그림을 포함한 전체 제작 진입점이다. **퍼즐 제작실**은 기존 본편의 퍼즐 한 판을 교체하는 이전 도구이며 별도로 유지한다.

| 탭 | 편집할 내용 |
| --- | --- |
| 프로젝트 | 새 이야기 추가, 기존 프로젝트 열기, 제목·제작자·엔딩 문구. 호텔 이야기를 복사해서 시작할 수도 있다. |
| 장소 | 장소 이름·설명·배경·해금 조사. 최대 30곳이며 플레이 화면에서 장소 목록을 넘길 수 있다. |
| 인물 | 주인공과 NPC의 이름·소개. 인물을 추가하고 PNG 그림을 연결한다. |
| 단서 | 수집품 이름·설명·그림. 조사 완료 보상으로 연결한다. |
| 조사 · 챕터 | 조사 이름·챕터·장소·선행 조사·보상·화면 좌표. 배경 미리보기를 클릭해 조사 위치를 지정한다. |
| 대사 | 선택한 조사의 전/후 대사를 따로 편집. 줄 추가·삭제·순서 이동, 화자·자세 선택. |
| 수수께끼 | 단일 정답/순서 문제, 질문·선택지·정답·힌트·해설. |
| 그림 | PNG 파일 선택 또는 경로 입력, 이미지 영역 자르기, 대화 인물의 좌우 방향 설정. 배경·인물·자세·단서·블록 슬롯을 교체한다. |

### 권장 제작 순서

1. 프로젝트 탭에서 **추가**로 빈 이야기를 만든다. 자동으로 시작 조사와 엔딩이 생긴다.
2. 장소·인물·단서를 만들고 그림을 넣는다. PNG는 프로젝트 폴더에 복사되므로 원본을 옮겨도 유지된다.
3. 조사 탭에서 **추가**하고 챕터 이름, 장소, 선행 조사, 보상 단서를 지정한다. 조사 종류는 대화/물건 조사, 블록 퍼즐, 수수께끼, 엔딩 중 선택한다.
4. **선택한 조사의 대사**에서 조사 전 대사와 조사 후 대사를 작성한다. 인물과 자세는 각 줄마다 선택한다.
5. 블록 퍼즐 조사에서 **퍼즐 편집 · 검증**을 열어 보드와 목표를 만들고 실제 합법적인 플레이로 클리어를 검증한다. 시험 플레이만으로는 등록되지 않는다. 저장 후 제작실로 돌아오면 인증도 프로젝트에 반영된다.
6. 순서대로 진행할 이야기라면 **목록 순서대로 조사 연결**을 누른다. 목록 이동만으로 기존 선행 조건을 자동 변경하지는 않는다.
7. **현재 조사 시험** 또는 **이야기 처음부터 시험**으로 확인한다. 시험은 별도 임시 저장에서 실행하며 상단 **제작실로 돌아가기**로 복귀한다.
8. **독립 MOD 내보내기**로 로컬 MOD 등록과 공유 ZIP 생성을 수행한다. 내보내기 전에 도달할 수 없는 조사, 조기 엔딩, 잘못된 그림, 미검증 퍼즐을 검사한다.

새 MOD는 `kind: adventure`로, 기본 호텔 이야기와 합치지 않고 자체 장소·인물·조사·퍼즐·엔딩으로 실행된다. 현재 게임의 2D 조사/대화/블록 퍼즐/수수께끼 규칙 안에서 새로운 게임 분량을 만들 수 있다. 임의의 새 장르나 새로운 규칙을 프로그래밍하는 범용 엔진은 아니다. 선택지에 따른 스토리 분기, 변수 조건, 여러 엔딩, 임의 UI 배치 편집, 온라인 업로드, 그림을 직접 그리는 브러시 도구는 아직 제공하지 않는다. 그림 편집은 PNG 적용·자르기·반전 범위다.

프로젝트는 사용자 데이터의 `StoryStudio/Projects/<프로젝트ID>/project.json`과 `Images`에 저장된다. 공유 ZIP은 `StoryStudio/Exports`, 설치된 MOD는 `Mods/adventure.<고유ID>`다. 내보낼 때 사용하는 퍼즐/수수께끼만 포함하므로 조사에서 연결을 해제한 초안은 인증을 요구하지 않는다. 프로젝트 자체에는 편집용 초안을 보존한다. 내보내기마다 새 MOD ID를 생성하므로 기존 배포본의 저장을 덮어쓰지 않는다.

UI는 `Assets/Game/Hotel/Prefabs/StoryStudio.prefab`, 씬의 `12 Story Studio · EDIT THIS PREFAB`에 저장되어 있다. Inspector와 Hierarchy에서 버튼·텍스트·미리보기·입력창 배치를 수정할 수 있으며 런타임 코드가 UI를 재생성하지 않는다. `HotelStoryStudio` 컴포넌트는 `HotelSystems`에 있고 뷰를 참조한다. 최초 추가용 Editor 메뉴는 이미 연결된 씬을 다시 만들지 않는다.

QA 재현: `-hotel-studio-review -screen-width 1600 -screen-height 900 -screen-fullscreen 0`. 배포 ZIP처럼 테스트용 테마가 빠진 경우 `-hotel-studio-art "<ClassicIllustrated/Images 절대 경로>"`를 추가한다. `StoryStudioReview`에 화면, 검사 결과, 자동 QA로 제작·클리어한 예제 MOD를 저장한다. 예제 인증은 자동 QA의 합법적인 플레이 기록이며 사용자의 수동 클리어 기록이 아니다.

검증 결과: EditMode **141/141 통과**. 실제 Windows 게임과 압축 해제한 순수 배포본에서 새 프로젝트 → 장소 4곳·챕터 2개 → 대사·조사·PNG 가져오기/자르기 → 퍼즐 클리어 인증 → 수수께끼 → 엔딩 → 제작실 복귀 → ZIP 내보내기 → 독립 MOD 선택을 통과했다. 본편 저장 보존과 기존 20개 조사·12개 퍼즐·4개 수수께끼·엔딩 회귀 검사도 통과했다. 파일 선택 창 조작 자체는 자동화하지 않았으며 PNG 경로 입력을 통해 같은 가져오기 처리를 검증했다. 증거는 `Builds/Hotel/Verification/story-studio-tests.xml`, `Builds/Hotel/StoryStudioReview`, `Builds/Hotel/Verification/StoryStudioPackaged-20261008`이다.

## MOD를 바꾸는 실제 플레이 영상

### 이야기 제작 → 독립 MOD 적용 → 새 게임과 엔딩 (2026-10-12)

`Builds/Hotel/StoryStudioDemo-20261012/MoonlitHotel-Create-MOD-and-Play-20261012.mp4` — **217.292초(3분 37초), 1600×900, 24fps, 무음**. 제작실에서 ‘비 오는 밤의 편지’를 새로 작성하고 실제 MOD 관리에서 선택한 뒤 처음부터 엔딩까지 플레이한다. 기존 본편 전체가 아닌 짧은 독립 이야기이며 조사 4개·퍼즐 1개·수수께끼 1개·대사 6줄을 포함한다.

- 00:00 제작 시작. 배경·인물 PNG 가져오기와 자르기, 단서·대사·조사 연결을 보여준다.
- 01:06 보드에 얼음을 배치하고 목표를 정한 뒤, 합법적인 플레이로 제작자 클리어를 검증한다.
- 01:35 수수께끼의 질문·선택지·정답·힌트·해설을 편집한다.
- 01:54 독립 MOD ZIP으로 내보낸다.
- **02:01 실제 MOD 선택**, 02:06 타이틀에서 새 게임 시작, 02:10 첫 조사.
- 02:27 제작한 블록 퍼즐, 02:52 수수께끼, **03:14 엔딩**, 03:25 단서, 03:33 엔딩 이어하기.

실제 저장된 UI의 자동 조작이며 강제 완료나 게임 구간 삭제가 없다. 입력 경로를 통해 기존 PNG를 가져온 것으로 그림 자체를 새로 제작하는 영상은 아니다. 사용자 요청으로 전체 화면에 **GCT 로고를 흰색·불투명도 20%·20도 대각선 반복**으로 합성했다. 안내 자막과 탐색 챕터를 추가했다. 최종 영상 전체 디코딩과 편집/대화 화면 표본 확인을 통과했다. 합법적인 일반 플레이 이동 8회, UI 클릭 110회, 독립 MOD 엔딩 저장 복원을 검증했다.

배포 파일: 다운로드 폴더의 `MoonlitHotel-Create-MOD-and-Play-GCT-20261012.mp4`, 구간 설명 TXT, `Rainy-Letter-Demo-MOD-20261012.zip`. 원본·클릭 시각·체크포인트·검증 기록은 녹화 폴더에 있다. 재현 옵션은 `-hotel-studio-demo -hotel-encoder <ffmpeg> -screen-width 1600 -screen-height 900 -screen-fullscreen 0`, 후처리는 `Tools/finalize_story_studio_demo.py --ffmpeg <ffmpeg> --watermark <GCT 로고 PNG>`다. 실제 사용자 저장은 사용하지 않는다.

### 최신: 중간 지점에서 MOD 전환 후 엔딩 완주 (2026-10-08)

`Builds/Hotel/MidpointModFullPlay/MoonlitHotel-FullPlay-MidpointMOD-20261008.mp4` — **554.583초(9분 14.6초), 1600×900, 24fps, 무음**. 새 게임부터 기본 상태로 10/20개 조사를 완료하고 클래식 MOD를 적용한다. 같은 저장으로 나머지 조사와 엔딩을 완료한다. 구간을 잘라 붙이지 않은 자동 UI 플레이이며 탐색용 챕터를 포함한다.

| 영상 시각 | 확인할 내용 |
| --- | --- |
| 3:39.3 | 기본 서윤·도식 대화, 도식의 설명 자세 |
| 3:44.5 | 전환 전 기본 복도 배경 |
| 3:51.5 | 전환 전 전화기 퍼즐 초기 배치·기본 블록·작은 인물 |
| 4:01.5 | 실제 MOD 관리 열기 |
| **4:05.5** | **클래식 MOD 선택 버튼 클릭** |
| 4:13.5 | 같은 저장 이어하기, 바뀐 복도 배경 |
| 4:20.5 | 같은 전화기 퍼즐 배치에서 바뀐 블록·얼음·작은 인물 |
| 4:45.9 | MOD 서윤·도식 대화, 도식의 같은 설명 자세 |
| 8:24.7 | 엔딩 조사와 마지막 대화 시작 |
| 8:41.0 | 엔딩 화면, 이후 모든 수집품 상세 확인 |
| 9:10.5 | 타이틀에서 이어하기로 MOD 상태의 엔딩 복원 |

이 MOD는 시각 테마다. 규칙·목표·초기 블록 배치·남은 횟수는 유지되고 그림이 교체된다. 동일한 퍼즐을 비교하기 위해 전환 직전 다음 판을 미리 열었다가 미완료 상태로 나왔으며, 완료한 10개 조사와 단서의 저장 파일 내용이 그대로인지 확인했다. 전환 후 같은 판을 실제로 풀고 나머지 진행을 완료했다.

검증: **20개 조사, 12개 퍼즐, 4개 수수께끼, 51줄 대사, 합법 이동 150회, 실제 UI 클릭 315회, 재도전 0회**. MOD 전후 저장 파일과 퍼즐 배치 일치, 엔딩까지 MOD 유지, 엔딩 저장 복원, 전체 영상 디코딩 통과. 전환 전후 퍼즐·대화 및 엔딩 화면도 표본 확인했다. 원본·타임라인·검증 보고서·화면은 같은 폴더에 보관한다.

재현: 기존 전체 녹화 옵션에 `-hotel-midpoint-mod`를 추가한다. `-hotel-full-play -hotel-mod-full-play -hotel-midpoint-mod -hotel-encoder <ffmpeg> -screen-width 1600 -screen-height 900 -screen-fullscreen 0`.

### 이전: 자세 그림 추가 전의 기본 플레이 → MOD 적용 → 엔딩 완주

대화 수정 후 다시 녹화한 최신 파일은 `Builds/Hotel/ModFullPlay/MoonlitHotel-Base-to-MOD-FullPlay-DialogueFixed.mp4`다. 아래 기본 파일명도 이 수정본으로 교체했다. MOD 일반 대화 캐릭터 교체, 문 선생의 시선, 도식의 축소·떠 있는 배치, 대사별 동작을 수정한 빌드로 같은 20개 조사·12개 퍼즐·4개 수수께끼·51줄 대사를 다시 완주했다. 전체 영상 디코딩 검사와 MOD 대화·도식·엔딩 표본 확인을 통과했다. 수정 전 영상은 `Builds/Backups/BeforeDialogueFix-20261006`에 보관한다.

`MoonlitHotel-Dialogue-Fixes-Preview.mp4`는 수정본에서 대화·동작·MOD 선택 장면을 모은 **66초 발췌 영상**이다. 전체 영상과 달리 구간 사이에 편집이 있다. 두 영상 모두 1600×900, 24fps, 무음이다. 인물별 기본/클래식 비교와 Inspector 편집 항목은 `Docs/VisualInvestigation.md`의 대화 인물 수정 항목을 참고한다.

`Builds/Hotel/ModFullPlay/MoonlitHotel-Base-to-MOD-FullPlay.mp4` — **537.958초(약 8분 58초), 1600×900, 24fps, 무음**, 탐색용 챕터 포함. MOD 없이 첫 조사를 정상 완료하고, 실제 MOD 관리에서 클래식 그림을 선택한 뒤 같은 저장으로 끝까지 이어간다. 이후 기본으로 되돌리지 않으며 엔딩 이어하기까지 클래식을 유지한다.

- 0:00 기본 상태로 새 게임, 첫 조사와 퍼즐.
- 0:43 MOD 관리로 이동, 0:46 클래식 선택, 0:49 선택 상태 확인.
- 0:52 같은 저장으로 이어하기, 0:59부터 나머지 캠페인 진행.
- 8:24 엔딩, 이후 전체 수집품 상세 확인.
- 8:53 타이틀에서 이어하기로 저장된 엔딩 복원.

검증 결과: **조사 20개, 블록 퍼즐 12개, 수수께끼 4개, 대사 51줄**, 합법적인 보드 이동 150회, 실제 UI 클릭 320회, 재도전 0회. MOD 적용 전후 저장 파일 내용 일치와 엔딩까지 MOD 유지 검사를 통과했다. 별도 임시 저장을 사용하는 자동 UI 플레이이며 강제 승리나 영상 구간 삭제는 없다. 전체 영상 디코딩 검사와 기본 조사·MOD 메뉴·MOD 대화·후반 퍼즐·엔딩 화면 표본 확인을 완료했다.

재현 옵션: `-hotel-full-play -hotel-mod-full-play -hotel-encoder <ffmpeg 실행 파일 경로> -screen-width 1600 -screen-height 900 -screen-fullscreen 0`. 원본, 타임라인, 검증 보고서, 주요 화면은 같은 `ModFullPlay` 폴더에 있다. 영상 정리는 `Tools/finalize_hotel_recording.py --folder Builds/Hotel/ModFullPlay --resolution 1600×900 --ffmpeg <ffmpeg>`로 한다. 이 녹화에서는 영상 종료 후 MOD를 기본으로 바꾸는 별도 연습 모드 감사를 실행하지 않는다.

### 이전: 짧은 왕복 전환 확인

`Builds/Hotel/ThemeSwitch/MoonlitHotel-MOD-ThemeSwitch.mp4` — **86.458초, 1600×900, 24fps, 무음**, 탐색용 챕터 포함. 첫 조사를 정상 완료한 뒤 실제 MOD 메뉴에서 클래식 → 기본을 선택한다. 각각 이어하기, 획득한 실 상세 보기, 다음 이야기 퍼즐에서 합법적인 클릭 1회, 미완료 퍼즐 나가기를 수행한다. 작은 퍼즐 인물·배경·블록·수집품이 바뀌는 모습을 확인할 수 있다.

- 0:00 기본 손그림으로 첫 조사와 퍼즐.
- 0:40 MOD 관리로 이동하여 클래식 테마 적용.
- 0:49 같은 진행을 이어서 클래식 수집품·퍼즐 확인.
- 1:02 MOD 관리로 돌아가 기본 그림 선택.
- 1:11 기본 그림의 수집품·퍼즐 확인.

검증 결과: 실제 UI 클릭 **51회**, 합법적인 보드 이동 **8회**, 첫 퍼즐 완료 **1개**, 완료 조사 **2개**. MOD 왕복 중 저장 파일 내용이 변하지 않았으며 추가 퍼즐은 완료하지 않고 나왔다. 실제 사용자 저장은 사용하지 않았다. 전체 영상 디코딩 통과. 전체 캠페인 완주 영상은 아니다.

재현 옵션: `-hotel-full-play -hotel-theme-showcase -hotel-encoder <ffmpeg 실행 파일 경로>`. 원본 영상·타임라인·검증 결과와 전환별 화면은 같은 `ThemeSwitch` 폴더에 있다. 이 옵션은 QA 자동 조작이며 일반 플레이 UI를 새로 생성하지 않는다.
