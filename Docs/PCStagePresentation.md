# PC 단일 화면 연출

전체 플레이 영상과 최신 재검증 결과는 [FullPlayVerification.md](FullPlayVerification.md)에 기록했다.

## 참고 영상에서 가져온 것

`C:/Users/GCT/Downloads/videoplayback.webm`은 약 9시간 30분 길이다. 전체 감상을 한 것은 아니며, 전 구간의 12개 시점과 849–859초 / 9398–9405초의 연속 프레임을 조사했다.

- 장소 안의 조사 → 큰 인물과 하단 대화창 → 다시 장소 조사로 돌아가는 흐름.
- 글자가 순서대로 나타나는 대사, 화자가 달라질 때 인물의 강조 변화.
- 걷기를 구현하지 않아도 장소 선택과 화면 전환으로 탐험이 성립하는 구조.

닌텐도의 두 화면 배치는 가져오지 않는다. PC에는 배경 한 장과 그 위의 인물, 하단 대화창만 있다. 중앙 분할선, 좌우 독립 화면, 지도와 대화의 이중 화면은 없다. 기존 조사 사이드바도 제거했다.

## 이번 구현

- 로비 / 복도 / 보관실 전체 화면 배경, 네 명의 교체 가능한 일러스트 시안.
- 같은 배경 위에 주인공과 상대 인물 표시. 인물 뒤에 별도 패널 없음.
- 대화 시작 시 조사 HUD 숨김. 인물 슬라이드·페이드 등장, 대화 종료 시 퇴장.
- 말하는 인물을 밝게 강조하고 미세한 상하 움직임 적용.
- 텍스트 메시의 투명도를 변경해 줄바꿈이 흔들리지 않는 순차 표시.
- 클릭 / Enter / Space: 표시 중에는 글자 완성, 완성 후에는 다음 대사.
- 장소 이동 시 암전 전환. 퍼즐 종료 대화도 퍼즐판 위가 아니라 해당 장소로 돌아와 진행.
- 저장 / MOD / 퍼즐과 조사 진행 구조 유지.

표정 교체, 입 모양, 손동작 프레임 애니메이션, 시선 제어는 아직 구현하지 않았다. 현재 그림은 연출을 확인하기 위한 시안이며 완성 캐릭터 디자인으로 확정한 것이 아니다.

## Hierarchy / Inspector 편집

씬: `Assets/Game/Scenes/Hotel.unity`

`Hotel Canvas · EDIT UI HERE / Safe Area · 1600x900` 아래:

- `02 Investigation · SINGLE FULL-SCREEN ROOM`: 배경, 조사 HUD, 장소 버튼.
- `09 Dialogue · actors on SAME room background`: 인물 Image 두 개, 하단 대화창, 이름표, 대사 Text, 계속 표시.
- `11 Room transition · editable curtain`: 장소 전환 색상.

`Hotel Systems · references / HotelStagePresentation`에서 글자 속도, 등장·퇴장 시간, 등장 거리, 호흡 이동량을 바꿀 수 있다. 인물 위치·크기 및 대화창 크기는 각각의 RectTransform에서 바꾼다. 기본 인물 이미지는 HotelGame의 Default Portraits, MOD 이미지는 기존 character.* 키를 사용한다.

실행 중 UI를 생성하지 않는다. Editor의 `UpgradePcStage`는 최초 1회 오브젝트를 만들어 씬에 저장하는 마이그레이션이다. 이미 적용한 씬에서는 재실행해도 재생성하지 않는다. 이전 씬은 `Builds/Backups/Hotel-before-PC-stage.unity`에 보관한다. 일반 빌드는 씬의 수동 수정을 보존한다.

## 검증용 실행

- `-hotel-smoke`: 독립 저장 폴더에서 전체 이야기, 퍼즐, MOD, 실제 UI 클릭 검증.
- `-hotel-preview`: 독립 저장 폴더에서 첫 대화를 실제 속도로 렌더링하고 `Builds/Hotel/PresentationPreview`에 연속 이미지 및 타임스탬프 저장. 첫 클릭이 대사를 넘기지 않고 표시만 완성하는지도 확인.

검증 결과: EditMode 113/113 통과. Windows 플레이어에서 실제 장소 페이드 대기까지 포함한 전체 20개 조사, 12개 블록 퍼즐, 4개 수수께끼, 저장/복원, MOD 검증 통과. 1280×720 대화 미리보기와 1600×900 전체 진행 확인. 증적은 `Builds/Hotel/Verification`에 보관했다. 미리보기 영상은 `Builds/Hotel/PresentationPreview/PC-dialogue-preview.mp4` (약 25초).

## 이미지 제작

imagegen 스킬의 내장 도구를 사용했다. 그림 파일은 모두 프로젝트 `Assets/Game/Hotel/Art/`에 복사되어 있으며 외부 사용자 폴더를 참조하지 않는다.

- `Characters-PC.png`: 네 인물의 투명 배경 아틀라스. 원본 생성 후 배경 제거·인물 간 여백 조정을 내장 편집 도구로 수행.
- `Portrait-you/front/keeper/guest.asset`: 아틀라스의 네 부분을 참조하는 Unity Sprite 자산.
- `Lobby-PC.png`, `Corridor-PC.png`, `Archive-PC.png`: 각 장소의 연속된 단일 배경.

프롬프트 방향: 독창적인 한국 산속 고호텔, 잉크 윤곽과 채색된 동화풍 2D, 호박색 램프와 푸른 저녁, 텍스트·UI·기존 작품 캐릭터 없음. 세 배경 모두 단일 공간 / 분할 없음. 인물은 서윤(단발·비옷·청록 목도리), 하린(적갈색 머리·버건디 조끼), 도식(회색 머리·남색 작업복), 문 선생(안경·올리브 카디건).

최종 생성/편집 프롬프트는 `PCStageArtPrompts.txt`에 기록했다.
