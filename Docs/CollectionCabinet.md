# 수집품 보관함

2026-10-06 후속: 현재 기본 그림은 `MysteryEvidence`와 `MysteryPieces`로 교체했다. 아래 `CollectionEvidence` 제작 기록의 원본은 클래식 테마 MOD에 보존했다. [현재 테마 구조](ThemeMods.md).

2026-10-06. 조사 수첩의 세로 텍스트 목록을 그림 중심의 3×3 보관함으로 변경했다. 카드에서 수집품을 확인하고, 선택하면 중앙의 큰 기록지에 물건 그림·발견 장소·설명을 표시한다. 좌우로 독립된 화면을 만들지 않는다.

- 기본 9개 기록(관찰 1개 + 사건 단서 8개)에 모두 그림을 연결했다. 흰 실과 열쇠는 기존 에셋을 재사용한다.
- 미발견 기록은 이름·그림을 숨기고 선택을 막는다. 빈 상태 안내와 수집 수 표시를 제공한다.
- 10~12개 기록은 두 번째 페이지에서 확인한다. 기존 MOD의 총 12개 제한은 유지한다.
- ESC는 상세 기록을 먼저 닫고, 한 번 더 누르면 이전 화면으로 돌아간다.
- 저장 파일의 형식, 보상, 단서 ID, 진행 조건은 변경하지 않았다. 기존 저장에서 흰 실 관찰은 계속 파생한다.
- MOD 그림 우선순위: `clue.<image>` → `clue.<id>` → 기존 내장 물건 그림 → 수집품 그림 → 범용 기록철.

## 편집

`Assets/Game/Scenes/Hotel.unity`의 `Hotel Canvas · EDIT UI HERE / Safe Area · 1600x900 / 05 Collection · evidence cabinet` 아래에 UI를 저장했다.

`Evidence slot 0~8`에서 카드 크기, 위치, 그림 영역과 텍스트를 수정한다. `Evidence detail · full overlay / Evidence folio · editable`에서 확대 기록지를 편집한다. `Hotel Systems · references / HotelCollectionView`는 카드 및 기본 그림 참조를 가진다. 대사와 마찬가지로 단서의 실제 제목·설명은 콘텐츠 JSON에서 갱신된다.

Editor의 `UpgradeCollection`은 명시적 최초 1회 마이그레이션이다. 이전 씬을 `Builds/Backups`에 보관하며, 적용된 씬에서 재실행해도 UI를 재생성하지 않는다. 일반 빌드는 수동 씬 편집을 유지한다. 런타임에 UI 오브젝트를 생성하지 않는다.

## 그림

imagegen 스킬의 내장 생성·편집 도구로 `Assets/Game/Hotel/Art/CollectionEvidence.png`를 만들었다. 인계표, 달 태그, 통화 기록, 메모, 안내도, 사진, 초대장, 범용 기록철의 8개 셀이다. PNG 픽셀은 별도로 가공하지 않았다. Unity Sprite 사각형을 각 물건의 알파 경계와 투명 간격에 맞춰 지정했다. `CollectionEvidence-00~07.asset`에서 각 Sprite를 참조한다.

최종 프롬프트는 [CollectionArtPrompts.txt](CollectionArtPrompts.txt)에 기록했다.

## 검증

`-hotel-collection-review`는 별도 임시 저장과 명시적인 UI 상태 샘플로 빈 보관함, 첫 발견 저장 복원, 전체 9개 상세 기록, 엔딩 복귀, 추가 기록 페이지와 MOD 그림 우선순위를 검사한다. 화면은 `Builds/Hotel/CollectionReview/<해상도>`에 저장한다. 전체 수집 화면은 UI 확인을 위해 구성한 상태이며 실제 사용자 저장을 변경하지 않는다.

전체 이야기 검증에는 기존 `-hotel-smoke`, 실제 첫 조사 진행 확인에는 `-hotel-full-play -hotel-first-slice`를 사용한다. 새 상세창을 닫고 수첩을 나가도록 녹화용 조작 순서도 함께 갱신했다.

이번 변경의 최종 확인: Windows 빌드 성공, EditMode 115/115 통과, 기본 이야기 20개 조사 및 저장·MOD smoke PASS. 1600×900과 1280×720에서 보관함 UI 검증 PASS(빈 상태 정보 보호, 기존 저장의 관찰 기록, 그림 9개와 상세창, 엔딩 복귀, 12개 기록의 페이지 이동, 범용 그림, MOD 그림 우선순위). 두 해상도의 실제 렌더 이미지를 확인했다. 이번에는 전체 플레이 영상이나 첫 조사 영상을 새로 녹화하지 않았으며, 기존 영상은 변경 전 화면일 수 있다.
