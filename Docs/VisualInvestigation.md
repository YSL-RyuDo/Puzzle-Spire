# 첫 조사와 퍼즐 화면 개편

2026-10-03. 참고 영상 분석 뒤, 첫 열쇠 보관함 조사를 시각적으로 연결하고 호텔 퍼즐 화면을 개편했다.

## 플레이에서 달라진 것

- 퍼즐: 큰 보드, 목표 그림과 남은 수, 낮은 이동 수 강조, 필요할 때 여는 도움말.
- 블록: 색과 도형을 함께 가진 새 그림, 방향이 보이는 로켓, 폭탄과 색 제거 그림. 상자와 얼음은 파손 상태로 구분한다. 내구도 2 이상만 숫자를 함께 표시한다.
- 입력: 일반 묶음에 커서를 올리면 함께 제거할 칸과 생성 가능한 특수 블록 종류를 안내한다. 힌트도 같은 표시를 사용한다.
- 한 턴: 특수 병합·변환·발동, 제거와 장애물 손상, 특수 생성, 낙하와 새 블록 진입. 클릭 영역과 장애물은 고정하고 블록 자식만 움직인다. 재배치가 발생하면 이전 중력 결과를 새 보드에 적용하지 않는다.
- 첫 조사: 하린과 대화 → 보관함 확대 → 퍼즐 → 흰 실 발견 → 인물 반응 → 그림이 있는 수첩. 서윤의 생각/발견, 하린의 걱정/회상 포즈를 추가했다.
- 로비의 벨과 시계를 별도로 조사할 수 있고, 보관함 조사를 마치면 완료 표시가 남는다.
- 실패 화면은 완료한 목표와 남은 개수를 구분한다.

공통 퍼즐 표현은 모든 스테이지에 적용한다. 새로운 확대 장면과 추가 인물 포즈는 첫 열쇠 보관함 구간에 집중했다. 다른 사건의 삽화, 모든 단서 그림, 풀 애니메이션, 음성/음악 제작은 포함하지 않는다.

## 저장과 MOD

`InvestigationNode`에 선택 필드 `inspection`, `discovery`, `discoveryTitle`, `discoveryText`를 추가했다. `DialogueLine.pose`, `ClueData.image`도 선택 필드다. 기존 JSON은 그대로 읽을 수 있다.

흰 실은 완료한 `keys` 노드에서 만들어 표시하는 관찰 기록이다. 저장 파일의 기존 `clues`, 노드 ID, 보상, 콘텐츠 버전을 변경하지 않았다. 이전 버전에서 보관함을 완료한 저장도 수첩에 흰 실 기록을 표시한다. 기존 기록과 관찰의 합계는 수첩 슬롯 수인 12개 이내로 검증한다.

MOD의 `block.*`와 `character.*` 이미지가 우선한다. 선택적으로 `character.you.curious`, `character.you.found`, `character.front.worried`, `character.front.remember`, `clue.thread`, `clue.key`를 사용할 수 있다. 수첩 이미지는 등록된 MOD 이미지가 없으면 내장 그림을 사용한다.

## 씬 저작

실행 중 Canvas나 UI GameObject를 생성하지 않는다. `HotelInvestigationView`에 그림, 목표 슬롯, 미리 배치한 효과 96개, 확대 화면, 소품 버튼이 연결되어 있다. `HotelCellView.blockRoot`가 움직이는 블록이며 장애물과 버튼은 그 바깥에 있다.

`HotelSceneBuilder.UpgradeInvestigation`은 명시적으로 실행하는 1회 마이그레이션이다. 기존 씬을 `Builds/Backups`에 보관한 뒤 해당 UI를 편집하고 저장한다. 이미 적용된 씬에서는 재실행해도 수동 편집을 보존한다. 일반 `CreateAndBuild`는 씬을 재생성하지 않는다.

퍼즐 규칙은 기존 Domain을 그대로 사용한다. 호텔의 uGUI 구조에 맞춘 연출에서 `PuzzleTurnResult`의 병합, 변환, 발동, 손상, 중력, 생성 데이터를 사용한다. 기존 월드 공간 `BoardView`를 런타임에 추가하지 않는다.

## 새 그림

내장 imagegen 도구로 생성했다. 모든 원본 PNG와 이를 참조하는 Sprite 자산을 프로젝트에 포함했다. 아틀라스의 픽셀은 수정하지 않고 각 셀의 알파 영역을 Sprite 사각형으로 지정했다.

- `Assets/Game/Hotel/Art/InvestigationPieces.png`: 일반 블록 5, 특수 블록 4, 상자 2, 얼음 2, 흰 실·열쇠·수건.
- `Assets/Game/Hotel/Art/KeyCabinet-PC.png`: 열쇠 보관함 확대 배경.
- `Assets/Game/Hotel/Art/InvestigationPoses.png`: 서윤과 하린의 추가 포즈 4개.
- 전체 생성 프롬프트: [InvestigationArtPrompts.txt](InvestigationArtPrompts.txt).

기본 콘텐츠 원본을 다시 저작할 때도 `Tools/hotel_visual_content.py`를 통해 같은 첫 조사 데이터를 적용한다. 이 스크립트만 실행하면 기존 `base.json`에서 해당 내용만 갱신한다.

## 2026-10-06 대화 인물 수정

MOD 활성화 시 콘텐츠 JSON을 복제하면서 생긴 빈 `portrait` 문자열도 기본 키(`character.<id>`)로 해석하도록 수정했다. 이전에는 표정이 없는 대사에서 MOD 그림 대신 기본 인물이 표시됐다. 일반 인물 4명과 표정 4종, MOD 해제 복원을 검사한다. 기본 그림에서 이미 왼쪽을 바라보는 문 선생은 오른쪽 반전 목록에서 제외했다.

대화 인물은 기존 Hierarchy의 Image를 그대로 사용한다. 두 인물의 pivot은 하단 중앙이며, 높이 690과 하단 위치를 공유하고 그림 비율에 따라 너비를 조정한다. 도식처럼 넓은 그림이 고정 폭 안에서 축소되어 위에 떠 있던 문제를 수정했다. 최대 너비 900으로 화면 밖으로 지나치게 커지는 것을 제한한다.

`Hotel Systems · references / HotelStagePresentation` Inspector에서 `Portrait Height`, `Maximum Portrait Width`를 편집한다. 위치와 하단 기준은 기존 `Seoyun · editable portrait slot`, `Conversation partner · editable portrait slot`의 RectTransform에서 편집한다. 런타임 UI 오브젝트 생성은 없다. 명시적 최초 배치 도구는 `UpgradeDialogueFraming`이며 일반 빌드는 사용자 배치를 다시 만들지 않는다.

요청을 재확인한 뒤, 임시로 추가했던 흔들기·기울이기 효과와 `motion` 데이터를 제거했다. 실제 자세 그림을 추가하고 `pose`로 교체한다. 인물별로 기본 그림 1종 + 대화 자세 2종을 사용하며, 51줄 모든 대사에 문맥에 맞는 자세를 지정했다. 기본 게임과 클래식 MOD 모두 같은 구성을 지원한다.

| 인물 | pose | 그림 |
| --- | --- | --- |
| 서윤 | `curious`, `found` | 생각하기, 발견하기 |
| 하린 | `worried`, `remember` | 걱정하기, 떠올리기 |
| 도식 | `think`, `explain` | 턱 짚기, 손가락 들어 설명하기 |
| 문 선생 | `remember`, `warm` | 책 펼쳐 회상하기, 가슴에 손을 얹고 웃기 |

새 기본 아틀라스: `Assets/Game/Hotel/Art/MysteryMalePoses.png`. 클래식 아틀라스: `Assets/Game/Hotel/Art/ClassicMalePoses.png` 및 외부 MOD의 `Images/ClassicMalePoses.png`. 내장 image_gen으로 기존 인물의 외형과 그림체를 참고해 제작했다. [프롬프트](DialoguePoseArtPrompts.md).

Inspector의 `HotelInvestigationView / Poses` 배열은 기존 여성 4장 다음에 도식 2장, 문 선생 2장 순서다. 기본 도식은 원본이 오른쪽을 보므로 `mirrorOnRight`에 새 2장을 등록했다. MOD는 이미지별 `flipInDialogue`로 반전을 명시할 수 있다. 기존 클래식 서윤의 발견 자세는 왼쪽을 보는 그림이므로 이 값을 true로 설정했다. 글자와 UI는 반전하지 않는다.

EditMode **124/124 통과**. 추가 회귀 검사에서 각 인물이 이야기에서 실제로 2가지 이상의 서로 다른 자세 그림을 사용하는지, MOD 자세를 불러오는지, 방향이 맞는지 확인했다. `-hotel-theme-review`는 모든 인물의 모든 대화 자세를 기본/클래식/기본 순서로 화면에 출력한다. 결과는 `ThemeReview/*-pose-*.png`다.

## 이전 검증 및 재현

- Unity 6000.3.1f1 EditMode: 115개 통과. 기존 저장 호환성과 관찰 기록을 포함한 수첩 수용량 검증을 추가했다.
- `-hotel-smoke`: 별도 임시 저장에서 전체 이야기, 퍼즐, 수수께끼, 저장과 MOD를 실제 UI 클릭으로 검증한다. `Builds/Hotel/Smoke`에 화면과 결과를 기록한다.
- `-hotel-effects-review`: 별도 임시 저장에서 가로/세로 로켓, 폭탄, 색 제거, 색 변환 조합, 특수 병합, 생성, 빈칸 보드의 8가지 명시적 테스트 보드를 사용한다. 실제 버튼 클릭, 연출 중 중복 입력 차단, 종료 후 그림과 도메인 상태 일치를 검사한다. `Builds/Hotel/PresentationReview`에 연속 프레임과 결과를 남긴다. 캠페인 플레이 영상과 구분된 합성 테스트다.
- `-hotel-full-play -hotel-first-slice -hotel-encoder "<ffmpeg.exe>"`: 첫 조사까지만 실제 UI로 진행하여 정상 속도 무음 영상을 남긴다. 수첩과 저장 복원도 확인한다. `Builds/Hotel/FirstInvestigation`에 영상, 시간표, 체크포인트와 결과를 기록한다. 첫 구간 녹화는 전체 캠페인 검증으로 표시하지 않는다.

빌드는 `Builds/Hotel/MoonlitHotel.exe`이며 실행에 동반 폴더가 필요하다. Unity 로그와 테스트 XML은 `Builds/Hotel/Verification`에 있다. 자동 검증은 사람에 의한 난이도·재미 평가를 대신하지 않는다.

최종 확인: Windows 빌드 성공, EditMode 115/115, 전체 이야기 자동 진행 PASS, 효과 테스트 8/8 PASS. 1600×900과 1280×720 실행 화면을 검토했다. 최종 첫 조사 영상은 1280×720, 24fps, 무음, 약 40초이며 실제 UI 클릭 23회와 유효한 퍼즐 수 6회로 진행했다. 저장 복원 후에도 흰 실 관찰 기록이 유지됐다. 영상 전체 디코딩 검사도 통과했다.

영상: `Builds/Hotel/FirstInvestigation/MoonlitHotel-FullPlay.mp4`. 이 경로의 영상은 첫 조사 구간만 담는다. `Verification/FirstInvestigationReview-*.jpg`에서 전체 영상의 1초 간격 표본을, `Verification/EffectsReview-*.jpg`에서 효과별 주요 프레임을 확인했다. UI 겹침을 수정한 뒤 최종 빌드와 플레이 영상을 다시 생성했다.
