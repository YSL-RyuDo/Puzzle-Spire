"""Add seek chapters and verify a completed Unity full-play recording. Requires ffmpeg."""
import argparse
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--ffmpeg', required=True)
parser.add_argument('--folder', default='Builds/Hotel/FullPlay')
args = parser.parse_args()
folder = Path(args.folder).resolve()
report = (folder / 'verification.txt').read_text(encoding='utf-8-sig')
if not report.startswith('PASS\n'):
    raise SystemExit('Recording did not pass; refusing to label an incomplete playthrough as reviewed.')
duration = float(next(line.split(': ', 1)[1] for line in report.splitlines() if line.startswith('Duration seconds:')))
chapters = [(0.0, '타이틀 · 새 조사 시작')]
for line in (folder / 'timeline.tsv').read_text(encoding='utf-8-sig').splitlines()[1:]:
    seconds, event = line.split('\t', 1)
    if event.startswith('Investigate: '):
        chapters.append((float(seconds), event.removeprefix('Investigate: ')))
    elif event == 'Ending reached':
        chapters.append((float(seconds), '엔딩 · 수첩 · 이어하기 검증'))


def escape(value):
    return value.replace('\\', '\\\\').replace('=', '\\=').replace(';', '\\;').replace('#', '\\#')


metadata = [';FFMETADATA1', 'title=잔월 호텔 · 새 게임부터 엔딩까지', 'comment=Automated UI play, silent, uninterrupted campaign.']
for index, (start, title) in enumerate(chapters):
    end = chapters[index + 1][0] if index + 1 < len(chapters) else duration
    metadata += ['[CHAPTER]', 'TIMEBASE=1/1000', f'START={round(start*1000)}', f'END={round(end*1000)}', 'title=' + escape(title)]
(folder / 'chapters.ffmetadata').write_text('\n'.join(metadata) + '\n', encoding='utf-8')
source = folder / 'MoonlitHotel-FullPlay.mp4'
final = folder / 'MoonlitHotel-FullPlay-Reviewed.mp4'
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
         f'- 영상: `{final.name}` / {timestamp(duration)} / 1280×720 / 24fps / 무음.',
         '- 실제 Windows 빌드에서 UI 버튼을 자동으로 눌러 플레이했다. 대사 생략, 강제 승리, 진행 순간이동, 영상 구간 삭제 없이 녹화했다.',
         '- 각 프레임을 고정 시간으로 렌더링해 인코딩 속도에 관계없이 영상에서는 원래 연출 시간으로 재생된다.',
         '- 수수께끼 오답→힌트→정답, 엔딩 이후 수첩, 타이틀 복귀→이어하기를 포함한다.',
         '- 영상 종료 후 연습 모드의 나가기 취소/확정, MOD 적용 및 기본 저장 복원을 추가 검사했다.',
         '- 사람의 수동 플레이나 난이도 평가는 아니다. 자동 진행은 보드 상태와 정답 데이터를 사용해 다음 클릭을 선택했다.',
         '', '## 결과', '', '```text', report.strip(), '```', '', '## 탐색 시점', '']
notes += [f'- {timestamp(t)} — {title}' for t, title in chapters]
notes += ['', '전체 프레임 디코딩 검사 통과. 원본 영상, 클릭 기록(timeline.tsv), 주요 화면(Checkpoints), 검증 결과(verification.txt)를 같은 폴더에 보관한다.', '']
(folder / 'REVIEW.md').write_text('\n'.join(notes), encoding='utf-8')
print(f'PASS: {final}\nDuration: {timestamp(duration)}; chapters: {len(chapters)}; bytes: {final.stat().st_size}')
