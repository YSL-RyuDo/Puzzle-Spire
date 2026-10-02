# 전체 진행 재검증 및 플레이 영상

2026-10-02, Windows 빌드에서 확인.

영상: `Builds/Hotel/FullPlay/MoonlitHotel-FullPlay-Reviewed.mp4`

- 7분 40초, 1280×720, 24fps, 무음, 탐색 챕터 22개.
- 타이틀 → 새 조사 → 전체 이야기 → 엔딩 → 모든 단서 확인 → 타이틀 → 이어하기.
- 조사 20개, 블록 퍼즐 12개, 수수께끼 4개, 대사 52개를 모두 진행했다.
- 유효한 퍼즐 수 150회, 재도전 0회, 녹화 중 UI 클릭 296회.
- 수수께끼마다 오답을 먼저 제출해 진행되지 않는 것을 확인하고, 힌트·선택 초기화·정답 제출로 진행했다.
- 영상 종료 후 연습 모드, 나가기 취소/확정, MOD 적용, MOD별 저장 분리, 기본 저장 복귀도 통과했다.
- EditMode 테스트 113/113 통과. 최종 영상 전체 디코딩 검사도 통과했다.

실제 게임 버튼을 자동으로 클릭한 영상이다. UI가 활성화되어 있고 다른 패널에 가리지 않았는지 확인한 뒤 클릭한다. 강제 승리·진행 상태 덮어쓰기·대사 생략·영상 구간 삭제를 사용하지 않았다. 보드 상태와 정답 데이터를 알고 선택하는 자동 진행이므로 사람의 수동 플레이 또는 난이도 평가를 대체하지 않는다.

첫 녹화에서는 마지막 대화의 종료 코루틴이 엔딩으로 전환된 뒤 녹화 제어 루프가 이전 상태라고 판단하는 오류가 있었다. 게임 자체는 엔딩에 도달했다. 제어 루프가 프레임 대기 이후 엔딩 상태를 다시 확인하도록 수정하고 전체 재녹화를 통과했다.

UI, 퍼즐 규칙, 콘텐츠 진행을 녹화용으로 새로 만들지 않았다. `HotelFullPlay.cs`는 명시적인 `-hotel-full-play` 인자로만 활성화되며, 별도 임시 저장 폴더를 사용한다. `HotelPresentationClock`은 평소에는 기존 비배속 시간으로 동작하고 녹화 때만 프레임당 1/24초로 고정한다. 인코더 처리 속도와 관계없이 영상에는 연출이 정상 속도로 기록된다. 런타임 UI 생성은 추가하지 않았다.

## 파일

- `Builds/Hotel/FullPlay/REVIEW.md`: 챕터별 시간과 결과.
- `Builds/Hotel/FullPlay/timeline.tsv`: 클릭 및 진행 타임스탬프.
- `Builds/Hotel/FullPlay/Checkpoints`: 모든 퍼즐·수수께끼·대사와 주요 화면.
- `Builds/Hotel/FullPlay/verification.txt`: 플레이 검증 결과.
- `Builds/Hotel/FullPlay/decode-check.txt`: 영상 전체 디코딩 결과.
- `Builds/Hotel/Verification/FullPlay.log`, `EditMode.xml`: 실행/테스트 증적.

## 재현

Unity Editor의 `PuzzleSpire.Hotel.Editor.HotelSceneBuilder.CreateAndBuild`로 빌드한 뒤 Windows 플레이어를 다음 인자로 실행한다. 인코더 경로는 설치한 ffmpeg 실행 파일을 지정한다. 평소 게임 실행에는 ffmpeg가 필요 없다.

```text
-hotel-full-play -hotel-encoder "<ffmpeg.exe 절대 경로>" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
```

완료 후 Python으로 `Tools/finalize_hotel_recording.py --ffmpeg "<ffmpeg.exe 절대 경로>"`를 실행하면 검증 결과가 PASS일 때만 최종 챕터 영상을 만들고 전체 디코딩을 검사한다.

남은 제작 범위: 입 모양·표정 애니메이션, 최종 그래픽·음악, 사람 플레이를 통한 난이도 및 재미 조정. 이번 검증은 현재 기본 콘텐츠의 전체 진행과 주요 부가 흐름에 대한 것으로 모든 가능한 입력을 검증한 것은 아니다.
