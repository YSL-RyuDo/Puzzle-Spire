"""Add seek chapters and verify a completed Unity full-play recording. Requires ffmpeg."""
import argparse
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--ffmpeg', required=True)
parser.add_argument('--folder', default='Builds/Hotel/FullPlay')
parser.add_argument('--resolution', default='1280×720')
parser.add_argument('--output', help='Output MP4 filename inside the recording folder')
args = parser.parse_args()
folder = Path(args.folder).resolve()
report = (folder / 'verification.txt').read_text(encoding='utf-8-sig')
if not report.startswith('PASS\n'):
    raise SystemExit('Recording did not pass; refusing to label an incomplete playthrough as reviewed.')
duration = float(next(line.split(': ', 1)[1] for line in report.splitlines() if line.startswith('Duration seconds:')))
mod_campaign = '> actual MOD menu > classic theme > same save continued to ending' in report
midpoint_mod = 'Midpoint MOD switch after 10/20 investigations' in report
chapters = [(0.0, '타이틀 · 새 조사 시작')]
for line in (folder / 'timeline.tsv').read_text(encoding='utf-8-sig').splitlines()[1:]:
    seconds, event = line.split('\t', 1)
    if event.startswith('Investigate: '):
        chapters.append((float(seconds), event.removeprefix('Investigate: ')))
    elif event == 'Ending reached':
        chapters.append((float(seconds), '엔딩 · 수첩 · 이어하기 검증'))
    elif event == 'MOD menu: base selected before switching':
        chapters.append((float(seconds), 'MOD 관리 · 클래식 적용'))
    elif midpoint_mod and event == 'click: MOD manager':
        chapters.append((float(seconds), 'MOD 관리 열기 · 기본 상태 확인'))
    elif event == 'Continue same campaign with classic MOD through ending':
        chapters.append((float(seconds), 'MOD 적용 후 같은 저장 이어하기'))
    elif event == 'Midpoint MOD activation click: base -> moonlight.classic':
        chapters.append((float(seconds), '전환 순간 · 클래식 MOD 선택'))
    elif event.startswith('Midpoint comparison: BASE corridor'):
        chapters.append((float(seconds), '전환 전 · 기본 복도 배경'))
    elif event.startswith('Midpoint comparison: BASE phone puzzle'):
        chapters.append((float(seconds), '전환 전 · 기본 퍼즐 블록과 주인공'))
    elif event.startswith('Midpoint comparison: MOD same phone puzzle'):
        chapters.append((float(seconds), '전환 후 · 같은 퍼즐의 MOD 블록과 주인공'))
    elif midpoint_mod and event.startswith('Dialogue: hall_trace / 도식 /'):
        chapters.append((float(seconds), '전환 전 · 기본 도식의 설명 자세'))
    elif midpoint_mod and event.startswith('Dialogue: phone_base / 도식 /'):
        chapters.append((float(seconds), '전환 후 · MOD 도식의 설명 자세'))


def escape(value):
    return value.replace('\\', '\\\\').replace('=', '\\=').replace(';', '\\;').replace('#', '\\#')


# Multiple checkpoints may describe the same frame. Keep one usable seek point.
chapters = list(dict(chapters).items())
metadata = [';FFMETADATA1', 'title=잔월 호텔 · 새 게임부터 엔딩까지', 'comment=Automated UI play, silent, uninterrupted campaign.']
for index, (start, title) in enumerate(chapters):
    end = chapters[index + 1][0] if index + 1 < len(chapters) else duration
    metadata += ['[CHAPTER]', 'TIMEBASE=1/1000', f'START={round(start*1000)}', f'END={round(end*1000)}', 'title=' + escape(title)]
(folder / 'chapters.ffmetadata').write_text('\n'.join(metadata) + '\n', encoding='utf-8')
source = folder / 'MoonlitHotel-FullPlay.mp4'
final = folder / (args.output or ('MoonlitHotel-Base-to-MOD-FullPlay.mp4' if mod_campaign else 'MoonlitHotel-FullPlay-Reviewed.mp4'))
subprocess.run([args.ffmpeg, '-y', '-i', str(source), '-i', str(folder / 'chapters.ffmetadata'),
                '-map', '0', '-map_metadata', '1', '-map_chapters', '1', '-c', 'copy', '-movflags', '+faststart', str(final)],
               check=True, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)
decoded = subprocess.run([args.ffmpeg, '-v', 'error', '-xerror', '-i', str(final), '-map', '0:v:0', '-f', 'null', '-'],
                         check=True, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)
(folder / 'decode-check.txt').write_text('PASS: complete video decoded without errors.\n' + decoded.stderr.decode('utf-8', errors='replace'), encoding='utf-8')


def timestamp(seconds):
    value = int(seconds)
    return f'{value//60:02d}:{value%60:02d}'


notes = ['# 잔월 호텔 풀 플레이 검증', '',
         f'- 영상: `{final.name}` / {timestamp(duration)} / {args.resolution} / 24fps / 무음.',
         '- 실제 Windows 빌드에서 UI 버튼을 자동으로 눌러 플레이했다. 대사 생략, 강제 승리, 진행 순간이동, 영상 구간 삭제 없이 녹화했다.',
         '- 각 프레임을 고정 시간으로 렌더링해 인코딩 속도에 관계없이 영상에서는 원래 연출 시간으로 재생된다.',
         '- 수수께끼 오답→힌트→정답, 엔딩 이후 수첩, 타이틀 복귀→이어하기를 포함한다.',
         ('- 기본 상태로 10/20개 조사를 마친 중간 지점에서 실제 MOD 관리로 클래식 그림을 선택했다. 같은 복도와 같은 전화기 퍼즐의 초기 배치를 전후로 보여주며 저장 진행과 퍼즐 규칙이 유지되는지 확인했다. 남은 캠페인부터 엔딩 이어하기까지 MOD를 유지했다. 이 MOD는 그림 테마이며 퍼즐 규칙을 바꾸지 않는다.' if midpoint_mod else '- MOD 없이 첫 조사를 마친 뒤 실제 MOD 관리에서 클래식 그림을 선택했다. 같은 저장으로 나머지 캠페인과 엔딩을 완료하고 엔딩 이어하기까지 MOD를 유지했다.' if mod_campaign else '- 영상 종료 후 연습 모드의 나가기 취소/확정, MOD 적용 및 기본 저장 복원을 추가 검사했다.'),
         '- 사람의 수동 플레이나 난이도 평가는 아니다. 자동 진행은 보드 상태와 정답 데이터를 사용해 다음 클릭을 선택했다.',
         '', '## 결과', '', '```text', report.strip(), '```', '', '## 탐색 시점', '']
notes += [f'- {timestamp(t)} — {title}' for t, title in chapters]
notes += ['', '전체 프레임 디코딩 검사 통과. 원본 영상, 클릭 기록(timeline.tsv), 주요 화면(Checkpoints), 검증 결과(verification.txt)를 같은 폴더에 보관한다.', '']
(folder / 'REVIEW.md').write_text('\n'.join(notes), encoding='utf-8')
print(f'PASS: {final}\nDuration: {timestamp(duration)}; chapters: {len(chapters)}; bytes: {final.stat().st_size}')
