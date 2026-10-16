"""Annotate and validate the actual Unity story-creation demonstration, without cutting frames."""
import argparse
import shutil
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--ffmpeg', required=True)
parser.add_argument('--watermark', type=Path)
parser.add_argument('--preview-time', type=float)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
folder = root / 'Builds/Hotel/StoryStudioDemo-20261012'
report = (folder / 'verification.txt').read_text(encoding='utf-8-sig')
assert report.startswith('PASS\n'), report
duration = float(next(line.split(': ', 1)[1] for line in report.splitlines() if line.startswith('Duration seconds:')))
chapters = []
for line in (folder / 'timeline.tsv').read_text(encoding='utf-8-sig').splitlines()[1:]:
    time, event = line.split('\t', 1)
    if event.startswith('Demo: '):
        chapters.append((float(time), event[6:]))
    elif event.startswith('Investigate: '):
        chapters.append((float(time), '플레이 · ' + event[13:]))
chapters = sorted(dict(chapters).items())

def ass_time(seconds):
    ticks = round(seconds * 100)
    return f'{ticks//360000}:{ticks//6000%60:02d}:{ticks//100%60:02d}.{ticks%100:02d}'

def stamp(seconds):
    return f'{int(seconds)//60:02d}:{int(seconds)%60:02d}'

metadata = [';FFMETADATA1', 'title=이야기 제작부터 MOD 적용과 엔딩까지', 'comment=Actual Unity build, automated editor input and legal play, silent. No cuts.']
subtitle = ['[Script Info]', 'ScriptType: v4.00+', 'PlayResX: 1600', 'PlayResY: 900', '[V4+ Styles]',
            'Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding',
            'Style: Default,NanumGothic,21,&H00FFFFFF,&H00FFFFFF,&H0023363D,&H0023363D,0,0,0,0,100,100,0,0,3,3,0,8,20,20,5,1',
            '[Events]', 'Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text']
for i, (start, title) in enumerate(chapters):
    end = chapters[i+1][0] if i + 1 < len(chapters) else duration
    metadata += ['[CHAPTER]', 'TIMEBASE=1/1000', f'START={round(start*1000)}', f'END={round(end*1000)}', 'title=' + title]
    subtitle.append(f'Dialogue: 0,{ass_time(start)},{ass_time(min(end, start+3))},Default,,0,0,0,,{title}')
(folder / 'chapters.ffmetadata').write_text('\n'.join(metadata)+'\n', encoding='utf-8')
(folder / 'guide.ass').write_text('\n'.join(subtitle)+'\n', encoding='utf-8')
fontdir = folder / 'VideoFonts'
fontdir.mkdir(exist_ok=True)
shutil.copyfile(root / 'Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf', fontdir / 'NanumGothic-Regular.ttf')
final = folder / 'MoonlitHotel-Create-MOD-and-Play-20261012.mp4'
watermark_input = ['-i', str(args.watermark.resolve())] if args.watermark else []
filters = ['-map', '0:v:0', '-vf', 'subtitles=guide.ass:fontsdir=VideoFonts']
if args.watermark:
    # Repeat the supplied logo as a diagonal, white, 20%-opacity video watermark.
    positions = [(x, y) for row, y in enumerate((-60, 275, 610))
                 for x in ((-150, 390, 930, 1470) if row % 2 == 0 else (-390, 150, 690, 1230))]
    graph = ['[0:v]subtitles=guide.ass:fontsdir=VideoFonts[scene]',
             '[2:v]format=rgba,colorkey=0xFFFFFF:0.12:0.04,scale=440:-1,'
             'colorchannelmixer=rr=0:rg=0:rb=0:ra=1:gr=0:gg=0:gb=0:ga=1:br=0:bg=0:bb=0:ba=1:aa=0.20,'
             'rotate=-20*PI/180:ow=rotw(-20*PI/180):oh=roth(-20*PI/180):c=none,'
             'split=' + str(len(positions)) + ''.join(f'[logo{i}]' for i in range(len(positions)))]
    previous = 'scene'
    for i, (x, y) in enumerate(positions):
        current = 'out' if i == len(positions)-1 else f'watermarked{i}'
        graph.append(f'[{previous}][logo{i}]overlay=x={x}:y={y}:format=auto[{current}]')
        previous = current
    filters = ['-filter_complex', ';'.join(graph), '-map', '[out]']
if args.preview_time is not None:
    subprocess.run([args.ffmpeg, '-v', 'error', '-y', '-i', 'MoonlitHotel-FullPlay.mp4',
                    '-i', 'chapters.ffmetadata', *watermark_input, *filters, '-ss', str(args.preview_time), '-frames:v', '1', '-update', '1', 'watermark-preview.png'],
                   cwd=folder, check=True)
    print(str(folder / 'watermark-preview.png'))
    raise SystemExit(0)
with (folder / 'finalize.log').open('w', encoding='utf-8') as log:
    subprocess.run([args.ffmpeg, '-y', '-i', 'MoonlitHotel-FullPlay.mp4', '-i', 'chapters.ffmetadata', *watermark_input,
                    *filters, '-map_metadata', '1', '-map_chapters', '1',
                    '-an', '-c:v', 'libx264', '-crf', '20', '-preset', 'fast', '-pix_fmt', 'yuv420p', '-movflags', '+faststart', final.name],
                   cwd=folder, check=True, stdout=subprocess.DEVNULL, stderr=log)
decoded = subprocess.run([args.ffmpeg, '-v', 'error', '-xerror', '-i', str(final), '-map', '0:v:0', '-f', 'null', '-'],
                         check=True, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)
(folder / 'decode-check.txt').write_text('PASS: full annotated video decoded without errors.\n'+decoded.stderr.decode(errors='replace'), encoding='utf-8')
notes = ['이야기 제작 → 실제 MOD 적용 → 새 게임부터 엔딩', '',
         f'{stamp(duration)} / 1600×900 / 24fps / 무음',
         '실제 Unity 게임의 입력창과 버튼을 자동 조작한 시연입니다. 사람의 수동 플레이 영상은 아닙니다.',
         '상단의 짧은 안내 자막과 탐색용 챕터를 추가했습니다. 게임 구간 삭제나 강제 클리어는 없습니다.',
         'GCT 센터 로고: 화면 전체 대각선 반복, 각 로고 너비 440px, 기울기 20도, 흰색 불투명도 20%, 원본 흰 배경 투명 처리.' if args.watermark else '워터마크 없음.',
         'PNG는 기존 에셋을 가져왔으며, 그림을 새로 그리는 과정은 아닙니다. 가져오기 경로 입력 방식을 사용했습니다.',
         '예제는 조사 4개, 블록 퍼즐 1개, 수수께끼 1개인 짧은 독립 이야기입니다. 기존 호텔 본편 전체가 아닙니다.',
         '', '구간 안내:'] + [f'{stamp(t)}  {title}' for t, title in chapters] + ['', report]
(folder / 'Video-Guide.txt').write_text('\n'.join(notes), encoding='utf-8')
print(str(final))
print(f'Duration: {duration:.3f} seconds; size: {final.stat().st_size}; complete decode PASS')
print('\n'.join(f'{stamp(t)} {title}' for t,title in chapters))
