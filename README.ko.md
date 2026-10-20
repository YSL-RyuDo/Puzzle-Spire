# Moonlit Hotel

<!-- portfolio-languages -->
[![English](https://ysl-ryudo.github.io/assets/diagrams/language-en.svg)](README.md) [![한국어 (현재 언어)](https://ysl-ryudo.github.io/assets/diagrams/language-ko-current.svg)](README.ko.md) [![日本語](https://ysl-ryudo.github.io/assets/diagrams/language-ja.svg)](README.ja.md)
<!-- /portfolio-languages -->

조사, 대화, 퍼즐을 결합하고 로컬 스토리 제작을 지원하는 미스터리 어드벤처입니다.

| 프로젝트 정보 | 내용 |
|---|---|
| 개발 상태 | 플레이 가능한 프로토타입 |
| 사용 기술 | C#, Unity, JSON / PNG |

[개발 사례 보기](https://ysl-ryudo.github.io/ko/work/moonlit/), [Portfolio](https://ysl-ryudo.github.io/ko/)

## 게임에서 보는 구현

[![유효한 교환과 목표 개수, 남은 이동 횟수를 연결한 매치 3 퍼즐 화면.](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)

유효한 교환과 목표 개수, 남은 이동 횟수를 연결한 매치 3 퍼즐 화면.

[![이야기 노드의 순서에 맞춰 대사와 인물 표시가 진행됩니다.](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)

이야기 노드의 순서에 맞춰 대사와 인물 표시가 진행됩니다.

[플레이 영상 보기](https://ysl-ryudo.github.io/ko/work/moonlit/#gameplay), 첫 조사와 대화, 퍼즐, 단서 수집, 저장 후 이어하기, 40초. 2026년 10월 20일 게임 내부 입력으로 자동 녹화했습니다.

## 게임 전체에서 맡은 구현

Moonlit Hotel은 조사와 이야기 진행, 매치 퍼즐 규칙, 사용자가 직접 만드는 제작 도구를 포함합니다. 규칙 Resolver와 MOD 형식 외에도 캠페인 흐름과 단서 수집, 퍼즐 표시, 이야기 편집과 퍼즐 시험을 구현했습니다.

[![담당 구현 흐름](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-ko.svg)](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-ko.svg)

### 조사와 이야기 진행

새 게임과 이어하기, 장소와 노드 진행, 대화와 수수께끼, 결말을 구현했습니다. 조사한 객체와 수첩 및 수집 항목을 기록해 월드 상호작용을 플레이어의 누적 단서에 연결했습니다.

[HotelGame.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelGame.cs), [내 변경 65c3791](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/65c379100887602f8a06cbcfb8edf00d0d512ea6), [HotelInvestigation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelInvestigation.cs), [내 변경 26d8094](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/26d8094e20e400818c2576b1406bdb24f4adf7cf)

### 퍼즐 규칙과 턴 연출

매치와 특수 블록, 장애물과 목표의 규칙 계층, 이동과 제거 및 효과의 표시 계층을 구현했습니다. 화면의 콜백만으로 규칙을 결정하지 않고 계산한 결과에 따라 턴을 표시합니다.

[HotelPuzzle.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Gameplay/HotelPuzzle.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelPuzzlePresentation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzlePresentation.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Story Studio 이야기 제작

이야기 노드와 선택지 편집, 조사와 수수께끼 입력 화면, 미리보기를 구현했습니다. 이야기 프로젝트를 검사하고 콘텐츠 팩으로 내보낼 수 있습니다. 이야기를 바꿀 때마다 개발자가 코드를 고치는 대신 제작과 시험을 제품 안에서 연결했습니다.

[HotelStoryStudioEditing.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelStoryStudioEditing.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Puzzle Workshop과 콘텐츠 저장

Puzzle Workshop에서 보드 편집과 프로젝트 저장 및 불러오기, 플레이 시험과 내보내기 검사를 구현했습니다. 외형만 바꾸는 테마와 내용을 바꾸는 콘텐츠의 저장 경계를 나눠 외형 변경 때문에 캠페인 진행이 불필요하게 분리되지 않도록 했습니다.

[HotelPuzzleWorkshop.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzleWorkshop.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [내 변경 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

## 기여 근거

[Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

## 구현과 설계

추리 어드벤처의 조사, 대화, 퍼즐을 연결하고, 새 이야기를 제작, 로드할 때 본편과 저장 상태가 섞이지 않도록 구성했습니다.

퍼즐 도메인 규칙과 표현을 분리하고 클리어 검증, 이야기 편집, 내보내기와 로컬 MOD별 독립 저장 상태를 구현했습니다.

## 코드의 동작과 구현 방식을 택한 이유

### 퍼즐 규칙을 화면 연출과 분리한 이유

**구현 흐름**

PuzzleTurnResolver는 보드 데이터를 대상으로 연결 블록 탐색, 제거, 중력, 생성, 특수 블록 규칙을 명시적인 의존성으로 조합합니다. 선택을 처리하기 전에 상태 머신이 입력을 허용하는지 확인하고 턴 결과를 반환합니다.

**이 구조가 필요한 이유**

애니메이션 완료 콜백 안에만 규칙을 넣으면 씬을 실행하지 않고 상태 변화를 확인하기 어렵습니다. C# 도메인 계층은 씬의 표시 없이 보드 전이를 검사할 수 있게 하고, 무엇이 일어났는지와 그것을 어떻게 보여줄지를 분리합니다. 입력 상태 검사도 턴을 시작할 수 있는 조건을 한곳에 드러냅니다.

**대안과 현재 구현의 절충**

작은 프로토타입에서는 MonoBehaviour 콜백에 규칙을 넣는 편이 연결 코드가 적습니다. 도메인 계층을 분리하면 연결 작업은 늘지만 모든 씬 조건을 재현하지 않고도 규칙을 변경하고 검사할 수 있습니다. 현재 Resolver는 이 분리를 사용하며, 표시 계층은 반환된 결과를 올바르게 소비해야 합니다.

**장단점과 검증 범위**

분리한 만큼 인터페이스와 결과 데이터가 늘어납니다. 입력과 애니메이션, 다음 턴이 같은 완료 시점을 사용하도록 규칙 계산 결과와 연출 시점을 맞춰야 합니다.

[Puzzle-Spire/PuzzleTurnResolver.cs](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Assets/Game/Domain/Puzzle/Rules/PuzzleTurnResolver.cs)

### 외형 교체와 콘텐츠 변경의 저장 경계를 나눈 이유

**구현 흐름**

테마 MOD는 아트를 교체하면서 기본 저장 데이터를 공유하고, 콘텐츠 MOD는 ID와 버전에 따른 별도 저장 경계를 사용합니다. 테마에서 콘텐츠 파일을 선언하는 것은 거부하고 에셋 경로, 영역, 이미지 제한을 검사하며, 교체할 에셋이 없으면 기본 에셋을 사용합니다.

**이 구조가 필요한 이유**

외형만 바꾼 경우에는 진행을 버릴 이유가 없지만 콘텐츠가 달라지면 기존 진행과 호환되지 않을 수 있습니다. 이를 형식에서 구분하면 우연히 들어 있는 파일을 보고 추정하는 대신 편집기와 실행 환경이 같은 저장 규칙을 적용할 수 있습니다.

**대안과 현재 구현의 절충**

모든 MOD의 저장을 분리하면 외형만 바꿔도 진행이 나뉩니다. 모두 공유하면 다른 콘텐츠가 같은 진행값을 서로 다르게 해석할 수 있습니다. 테마와 콘텐츠 형식을 나누면 이 차이를 명시하고 콘텐츠 변경에 버전 관리 부담을 집중할 수 있습니다.

**장단점과 검증 범위**

한 번에 MOD 하나만 적용해 교체 우선순위 충돌을 줄이는 대신 조합의 자유도가 제한됩니다. 콘텐츠 변경에는 저장 호환성과 버전 관리 규칙도 필요합니다.

[Puzzle-Spire/ThemeMods.md](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Docs/ThemeMods.md)

## 실행, 코드 확인

[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)의 Unity 버전으로 프로젝트를 여세요. 패키지 잠금 파일과 직렬화된 에셋을 유지하고, Unity를 열기 전에 문서에서 설명한 브랜치를 선택하세요.

Unity scene: [Assets/Game/Scenes/Hotel.unity](Assets/Game/Scenes/Hotel.unity)

## 개발, 검증 범위

플레이 가능한 프로토타입입니다. 기존 README의 2026 ~ 10 ~ 08 기록에는 EditMode 141개 통과가 있으며 이후 이야기 MOD 진행 기록도 있습니다. 합법적인 퍼즐 수를 이용한 자동 진행, UI 검사는 사람의 난이도, 재미 평가를 대신하지 않습니다. 제작실의 플랫폼 직접 업로드는 미구현이며 외부 ZIP은 MOD 폴더에 압축 해제합니다.

## 상세 문서

[기존 개발, 사용, 검증 상세 안내](docs/DEVELOPMENT_REFERENCE.ko.md)

[Docs/FullPlayVerification.md](Docs/FullPlayVerification.md)

[Docs/HotelPrototype.md](Docs/HotelPrototype.md)

[Docs/ThemeMods.md](Docs/ThemeMods.md)

## 관련 저장소

[Puzzle-Spire](https://github.com/YSL-RyuDo/Puzzle-Spire), 비공개, 접근 권한 필요
