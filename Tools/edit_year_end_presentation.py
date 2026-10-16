"""Build a captioned government-project presentation from the two supplied videos.

The already-watermarked supplied video is hash-matched to our earlier export; its
clean original is used to apply exactly one 15% watermark, without logo removal.
All trims, source references, captions, and checks remain in the output folder.
"""
import argparse
import concurrent.futures
import csv
import hashlib
import json
import shutil
import subprocess
from pathlib import Path
from PIL import ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Builds/Presentations/YearEnd-20261014'
FF = Path('C:/Users/GCT/AppData/Local/Programs/Python/Python310/Lib/site-packages/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe')
DOWNLOADS = Path('C:/Users/GCT/Downloads')
PROVIDED = DOWNLOADS / '[게임1]MOD게임 연말 발표용.mp4'
PRIOR = ROOT / 'Builds/Hotel/StoryStudioDemo-20261012/MoonlitHotel-Create-MOD-and-Play-20261012.mp4'
SOURCES = {'studio': ROOT / 'Builds/Hotel/StoryStudioDemo-20261012/MoonlitHotel-FullPlay.mp4', 'test': DOWNLOADS / '시연 테스트.mp4'}
LOGO = DOWNLOADS / 'GCT 센터 로고.png'
FONT = ROOT / 'Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf'
FINAL = OUT / 'GCT_MOD플랫폼_연말발표_통합본_20261014.mp4'
PROJECT = '연구개발과제명 : 게임콘텐츠 저작권 관리 기술 개발 및 글로벌 인재양성'
ACK = '본 연구는 문화체육관광부 및 한국콘텐츠진흥원의 2024년도 문화체육관광 연구개발사업으로 수행되었음(연구개발과제명 : 게임콘텐츠 저작권 관리 기술 개발 및 글로벌 인재양성, 연구개발과제번호 : RS-2024-00396709, 기여율: 100%)'
FPS = 24
parser = argparse.ArgumentParser()
parser.add_argument('--prepare-only', action='store_true')
parser.add_argument('--preview', action='store_true')
args = parser.parse_args()
OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'parts').mkdir(exist_ok=True)
(OUT / 'VideoFonts').mkdir(exist_ok=True)
shutil.copyfile(FONT, OUT / 'VideoFonts/NanumGothic-Regular.ttf')

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
assert sha(PROVIDED) == sha(PRIOR), 'Supplied studio video differs: do not substitute the clean original.'
assert all(p.exists() for p in [*SOURCES.values(), LOGO, FONT])

# Source ranges use seconds. Output durations are quantized once to exact 24fps frames.
spec = [
    ('black', 0, 10, 'GCT MOD 플랫폼 소개'),
    ('test', 224, 228, '01  플랫폼 · 게임 창작마당'),
    ('test', 204, 210, '01  플랫폼 · 원본 게임과 MOD 분리 등록'),
    ('test', 241, 249, '01  플랫폼 · MOD 정보 등록'),
    ('test', 228, 236, '01  플랫폼 · 원본 게임 다운로드'),
    ('test', 278, 284, '01  플랫폼 · 다운로드 내역'),
    ('black', 0, 10, '02  플랫폼 활용 사례 · MOD 적용 범위'),
    ('test', 311, 323, '02  플랫폼 활용 사례 · 퍼즐·추리 게임'),
    ('test', 537, 545, '03  그림 MOD · 적용 전 배경과 퍼즐'),
    ('test', 549, 566, '03  그림 MOD · 테마 선택과 적용'),
    ('test', 566, 583, '03  그림 MOD · 적용 후 배경과 퍼즐'),
    ('test', 818, 831, '03  그림 MOD · 대화 인물'),
    ('test', 840, 848, '03  그림 MOD · 수집품 이미지'),
    ('studio', 0, 117, '04  제작자 흐름 · 콘텐츠 MOD 제작과 내보내기'),
    ('studio', 117, 5215/FPS, '05  이용자 흐름 · 독립 이야기 MOD 적용과 플레이'),
    ('black', 0, 10, '플랫폼 활용 흐름 정리'),
    ('black', 0, 20, '사사'),
]
parts = []
frame = 0
for i, (source, start, end, heading) in enumerate(spec):
    frames = round((end-start)*FPS)
    parts.append(dict(index=i, source=source, source_start=start, source_end=end, frames=frames,
                      start=frame/FPS, end=(frame+frames)/FPS, heading=heading, file=f'parts/platform-{i:02d}.mp4'))
    frame += frames
DURATION = frame/FPS
ACK_START = parts[-1]['start']
manifest = dict(duration=DURATION, fps=FPS, size=[1920,1080], content_rect=[160,46,1600,900],
                project_caption=PROJECT, acknowledgement=ACK, watermark=dict(opacity=.15, angle=-20, repeated=True),
                supplied_studio_sha256=sha(PROVIDED), matching_prior_export_sha256=sha(PRIOR),
                clean_original_sha256=sha(SOURCES['studio']), sources={k:str(v) for k,v in SOURCES.items()}, parts=parts)
(OUT / 'edit-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')

captions = []
def caption(start, end, text):
    assert end > start
    assert ImageFont.truetype(str(FONT), 34).getlength(text) < 1730, text
    captions.append((start, end, text))

def part_caption(index, text):
    p=parts[index]; caption(p['start'],p['end'],text)
part_caption(0,'GCT 플랫폼을 중심으로 게임·MOD의 등록, 배포와 활용 사례를 소개합니다.')
part_caption(1,'플랫폼의 게임 창작마당에서 등록된 원본 게임을 확인합니다.')
part_caption(2,'개발자 센터는 원본 게임과 해당 게임의 MOD를 별도로 등록합니다.')
part_caption(3,'원본 게임을 지정하고 MOD 이름, 변경 내용과 버전 정보를 등록합니다.')
part_caption(4,'이용자는 게임 상세 화면에서 설치 안내를 확인하고 실행 파일을 내려받습니다.')
part_caption(5,'플랫폼의 내 다운로드 화면에서 내려받은 게임 내역을 확인합니다.')
part_caption(6,'플랫폼 활용 사례로 퍼즐·추리 게임을 MOD 적용이 가능한 구조로 확장했습니다.')
part_caption(7,'기본 게임은 인물 대화, 장소 조사, 퍼즐과 단서 수집으로 진행됩니다.')
part_caption(8,'그림 MOD 적용 전의 복도 배경과 퍼즐 블록을 확인합니다.')
part_caption(9,'게임의 MOD 관리에서 설치한 테마를 선택하고 기존 진행을 이어갑니다.')
part_caption(10,'퍼즐 배치와 목표는 유지하면서 배경과 블록 이미지가 교체됩니다.')
part_caption(11,'등장인물 이미지와 대화 자세에도 선택한 그림 MOD가 반영됩니다.')
part_caption(12,'캐릭터·배경·퍼즐 블록 외에 수집품 이미지도 MOD로 교체할 수 있습니다.')

studio_cues = [
    (0,8,'이야기 제작실에서 프로젝트 제목과 엔딩 문구를 입력합니다.'),
    (8,20,'장소를 추가하고 PNG 이미지를 불러와 배경으로 지정합니다.'),
    (20,30,'인물 이미지를 불러온 뒤 사용할 영역을 잘라 적용합니다.'),
    (30,36,'조사 보상으로 사용할 단서의 이름과 설명을 작성합니다.'),
    (36,46,'대사 줄을 추가하고 말하는 인물과 대사 내용을 지정합니다.'),
    (46,57,'조사에 장소, 퍼즐, 선행 조건과 단서 보상을 연결합니다.'),
    (57,64,'조사 전후의 대사를 나누어 이야기 흐름을 구성합니다.'),
    (64,69.417,'퍼즐 보드에 얼음을 배치하고 제거 목표를 설정합니다.'),
    (69.417,81.75,'제작자가 직접 퍼즐을 클리어하여 배포 전 검증을 수행합니다.'),
    (81.75,87,'검증한 퍼즐을 저장하고 다음 조사에 수수께끼를 연결합니다.'),
    (87,98,'수수께끼의 질문, 선택지, 정답, 힌트와 해설을 작성합니다.'),
    (98,107,'마지막 조사와 대사를 편집하여 엔딩을 구성합니다.'),
    (107,112,'조사 순서를 연결하고 프로젝트를 저장합니다.'),
    (112,117,'완성한 이야기를 로컬 MOD로 내보내고 공유용 ZIP 패키지를 만듭니다.'),
]
for a,b,t in studio_cues: caption(parts[13]['start']+a,parts[13]['start']+b,t)
play_cues = [
    (117,125,'제작실에서 내보낸 독립 이야기 MOD를 선택합니다.'),
    (125,131,'선택한 MOD로 새 게임을 시작하여 첫 조사부터 진행합니다.'),
    (131,140,'제작실에서 작성한 대사와 지정한 화자가 실제 게임에 나타납니다.'),
    (140,148,'연결한 조사 장소로 이동하고 해당 퍼즐을 시작합니다.'),
    (148,160,'제작한 보드와 목표를 그대로 불러와 일반 플레이로 진행합니다.'),
    (160,170,'퍼즐을 완료하면 작성한 후속 대사와 단서 보상이 적용됩니다.'),
    (170,183,'직접 작성한 수수께끼의 질문, 선택지와 정답 처리를 확인합니다.'),
    (183,195,'마지막 대화를 진행하면 제작한 이야기의 엔딩으로 연결됩니다.'),
    (195,201,'프로젝트에 입력한 엔딩 제목과 문구가 실제 화면에 표시됩니다.'),
    (201,209,'조사 보상으로 설정한 단서의 이름과 설명을 확인합니다.'),
    (209,5215/FPS,'이어하기로 MOD의 엔딩 진행 상태가 복원되는지 확인합니다.'),
]
for a,b,t in play_cues: caption(parts[14]['start']+a-117,parts[14]['start']+b-117,t)
part_caption(15,'플랫폼의 게임·MOD 분리 등록·배포와 게임 내 콘텐츠 확장 흐름을 시연했습니다.')
assert abs(captions[0][0]) < .001 and abs(captions[-1][1]-ACK_START)<.001
for prev,nxt in zip(captions,captions[1:]): assert abs(prev[1]-nxt[0]) < .001, (prev,nxt)

def ass_time(t):
    n=round(t*100); return f'{n//360000}:{n//6000%60:02d}:{n//100%60:02d}.{n%100:02d}'
def srt_time(t):
    n=round(t*1000);return f'{n//3600000:02d}:{n//60000%60:02d}:{n//1000%60:02d},{n%1000:03d}'
def stamp(t): return f'{int(t)//60:02d}:{int(t)%60:02d}'
styles = [
    '[Script Info]','ScriptType: v4.00+','PlayResX: 1920','PlayResY: 1080','WrapStyle: 2',
    '[V4+ Styles]',
    'Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding',
    'Style: Caption,NanumGothic,34,&H00FFFFFF,&H00FFFFFF,&H00131820,&H00131820,0,0,0,0,100,100,0,0,1,0,0,5,40,40,0,1',
    'Style: Header,NanumGothic,24,&H00DED3AE,&H00FFFFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,4,40,40,0,1',
    'Style: Footer,NanumGothic,32,&H00FFFFFF,&H00FFFFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,5,40,40,0,1',
    'Style: Card,NanumGothic,42,&H00FFFFFF,&H00FFFFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,5,80,80,0,1',
    '[Events]', 'Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text',
]
events=[]
def event(start,end,style,text,tags='',layer=1):
    events.append(f'Dialogue: {layer},{ass_time(start)},{ass_time(end)},{style},,0,0,0,,{{{tags}}}{text}')
event(0,DURATION,'Footer',PROJECT,r'\pos(960,1050)')
for p in parts[1:-1]: event(p['start'],p['end'],'Header',p['heading'],r'\pos(160,23)')
for start,end,text in captions: event(start,end,'Caption',text,r'\pos(960,984)')
event(0,10,'Card','GCT MOD 플랫폼 활용 시연',r'\pos(960,370)\fs64')
event(0,10,'Card','게임과 MOD의 등록·배포에서 이용자 적용까지',r'\pos(960,475)\fs36\c&HDDD5BF&')
event(0,10,'Card','제작자   →   GCT 플랫폼   →   이용자',r'\pos(960,590)\fs34')
def card(index, lines):
    p=parts[index]
    for text,y,size in lines: event(p['start'],p['end'],'Card',text,rf'\pos(960,{y})\fs{size}')
card(6,[('MOD로 확장한 퍼즐·추리 게임',295,52),
        ('그림 MOD  |  캐릭터 · 배경 · 퍼즐 블록 · 수집품 이미지',425,36),
        ('콘텐츠 MOD  |  대사 · 조사 · 퍼즐 · 수수께끼 · 엔딩',510,36),
        ('기존 게임을 기반으로 새로운 그림과 독립 이야기를 적용',635,32)])
card(15,[('플랫폼을 통한 게임·MOD 활용',315,52),
         ('제작자  :  원본 게임과 MOD를 분리하여 등록',440,36),
         ('플랫폼  :  콘텐츠 정보와 다운로드 경로 제공',520,36),
         ('이용자  :  MOD를 선택하여 그림과 이야기를 확장',600,36)])
event(ACK_START,DURATION,'Card','사사',r'\pos(960,250)\fs30\c&HDDD5BF&')
ack_lines=[
    '본 연구는 문화체육관광부 및 한국콘텐츠진흥원의',
    '2024년도 문화체육관광 연구개발사업으로 수행되었음',
    '(연구개발과제명 : 게임콘텐츠 저작권 관리 기술 개발 및 글로벌 인재양성,',
    '연구개발과제번호 : RS-2024-00396709, 기여율: 100%)',
]
assert ''.join(''.join(ack_lines).split()) == ''.join(ACK.split()), 'Acknowledgement text changed'
for i,line in enumerate(ack_lines):
    assert ImageFont.truetype(str(FONT),40).getlength(line)<1600
    event(ACK_START,DURATION,'Card',line,rf'\pos(960,{384+i*82})\fs40')
(OUT/'presentation.ass').write_text('\n'.join(styles+events)+'\n',encoding='utf-8')
(OUT/'기능설명_자막.srt').write_text('\n\n'.join(f'{i+1}\n{srt_time(a)} --> {srt_time(b)}\n{t}' for i,(a,b,t) in enumerate(captions))+'\n',encoding='utf-8')
with (OUT/'편집구간.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.writer(f);writer.writerow(['완성본 시작','완성본 끝','구간','원본','원본 시작 초','원본 끝 초'])
    for p in parts:writer.writerow([stamp(p['start']),stamp(p['end']),p['heading'],p['source'],p['source_start'],p['source_end']])
metadata=[';FFMETADATA1','title=GCT MOD 플랫폼 연말 발표 통합 시연','comment=Edited platform registration, distribution and game MOD use cases. GCT watermark 15%.']
for p in parts:
    metadata += ['[CHAPTER]','TIMEBASE=1/1000',f'START={round(p["start"]*1000)}',f'END={round(p["end"]*1000)}','title='+p['heading']]
(OUT/'chapters.ffmetadata').write_text('\n'.join(metadata)+'\n',encoding='utf-8')

def prepare(p):
    target=OUT/p['file']
    if target.exists(): return p['index']
    cmd=[str(FF),'-v','error','-y']
    if p['source']=='black': cmd+=['-f','lavfi','-i','color=c=black:s=1600x900:r=24']
    else: cmd+=['-ss',str(p['source_start']),'-i',str(SOURCES[p['source']])]
    vf='scale=1600:900:flags=lanczos'
    if p['index'] in (1,2,3,4,5):
        # Reframe the actual workshop UI above the unrelated terminal window.
        vf='crop=1660:540:248:96,scale=1600:-2:flags=lanczos,pad=1600:900:0:(oh-ih)/2:color=0x101820'
    cmd+=['-map','0:v:0','-vf',vf+',setsar=1,fps=24,setpts=PTS-STARTPTS',
          '-frames:v',str(p['frames']),'-an','-map_metadata','-1','-map_chapters','-1','-c:v','libx264','-preset','fast','-crf','16',
          '-pix_fmt','yuv420p','-threads','4',str(target)]
    subprocess.run(cmd,check=True,stderr=subprocess.PIPE)
    return p['index']

with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    for i in pool.map(prepare,parts): print(f'Prepared segment {i+1}/{len(parts)}',flush=True)
(OUT/'concat.txt').write_text('\n'.join("file '"+p['file']+"'" for p in parts)+'\n',encoding='utf-8')
if args.prepare_only:
    print(f'Prepared {DURATION:.3f} seconds. Acknowledgement begins {ACK_START:.3f}.',flush=True)
    raise SystemExit(0)

positions=[(x,y) for row,y in enumerate((-60,275,610)) for x in ((-150,390,930,1470) if row%2==0 else (-390,150,690,1230))]
graph=['[0:v]format=rgba[scene]',
       '[1:v]format=rgba,colorkey=0xFFFFFF:0.12:0.04,scale=440:-1,'
       'colorchannelmixer=rr=0:rg=0:rb=0:ra=1:gr=0:gg=0:gb=0:ga=1:br=0:bg=0:bb=0:ba=1:aa=0.15,'
       'rotate=-20*PI/180:ow=rotw(-20*PI/180):oh=roth(-20*PI/180):c=none,'
       'split=12'+''.join(f'[logo{i}]' for i in range(12))]
previous='scene'
wm_enable='+'.join(f'between(t,{p["start"]},{p["end"]-1/FPS})' for p in parts if p['source']!='black')
for i,(x,y) in enumerate(positions):
    current=f'wm{i}';graph.append(f'[{previous}][logo{i}]overlay=x={x}:y={y}:format=auto:enable=\'{wm_enable}\'[{current}]');previous=current
graph.append(f'[{previous}]pad=1920:1080:160:46:color=black,'
             f'drawbox=x=0:y=946:w=1920:h=74:color=0x101820:t=fill:enable=\'lt(t,{ACK_START})\','
             'drawbox=x=0:y=1020:w=1920:h=60:color=black:t=fill,'
             'drawbox=x=0:y=1020:w=1920:h=2:color=0xAA9566:t=fill,'
             'subtitles=presentation.ass:fontsdir=VideoFonts,format=yuv420p[out]')
(OUT/'filtergraph.txt').write_text(';'.join(graph),encoding='utf-8')
inputs=[str(FF),'-v','warning','-y','-f','concat','-safe','0','-i','concat.txt','-loop','1','-framerate','24','-i',str(LOGO),'-i','chapters.ffmetadata']
filters=['-filter_complex_script','filtergraph.txt','-map','[out]']
if args.preview:
    # Select frames after timestamp-sensitive filters to verify the actual captions.
    points=[4,16,35,76,118,174,202,231,260,299,ACK_START+8]
    for i,t in enumerate(points):
        subprocess.run(inputs+filters+['-ss',str(t),'-frames:v','1','-update','1',f'preview-{i:02d}.png'],cwd=OUT,check=True,stderr=subprocess.PIPE)
        print(f'Preview {i}: {t:.1f}s',flush=True)
    raise SystemExit(0)

with (OUT/'render.log').open('w',encoding='utf-8') as log:
    subprocess.run(inputs+filters+['-frames:v',str(frame),'-map_metadata','2','-map_chapters','2','-an','-c:v','libx264','-preset','fast','-crf','19',
                                  '-pix_fmt','yuv420p','-threads','6','-movflags','+faststart',str(FINAL)],cwd=OUT,check=True,stderr=log)
print('Render complete; decoding the entire file.',flush=True)
result=subprocess.run([str(FF),'-v','error','-xerror','-i',str(FINAL),'-map','0:v:0','-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE)
(OUT/'verification.txt').write_text('PASS: complete final video decoded without errors.\n'
    +f'Duration: {DURATION:.3f} seconds\nResolution: 1920x1080\nFPS: 24\n'
    +'Watermark: repeated diagonal GCT logo, alpha 0.15, applied once from clean originals.\n'
    +'Project caption: exact requested text; visible from first frame to end.\n'
    +'Acknowledgement: user-approved Korean institution spelling; all other requested wording retained.\n'
    +'Footage is edited, not a continuous live capture. Platform UI is reframed above the terminal.\n'
    +result.stderr.decode(errors='replace'),encoding='utf-8')
guide=['GCT MOD 플랫폼 연말 발표 통합본',f'길이: {stamp(DURATION)} / 1920×1080 / 24fps / 무음',
       '두 원본에서 플랫폼 등록·배포 화면, MOD 적용 사례와 제작 과정을 선별한 편집 영상입니다.',
       '제작실의 신규 이야기 ZIP 내보내기는 로컬 시연이며, 해당 ZIP의 플랫폼 업로드를 촬영한 것은 아닙니다.',
       '대각선 반복 GCT 워터마크 15%, 하단 기능 설명, 맨 아랫줄 과제명 상시 표기.',
       '기존 20% 워터마크 파일과 동일한 영상임을 해시로 확인한 뒤 보관된 무워터마크 원본을 사용했습니다.',
       '마지막 검정 사사 화면은 20초이며, 기관명은 사용자 확인에 따라 한국콘텐츠진흥원으로 수정했습니다.',
       '',PROJECT,'',ACK,'','구간 안내:']
guide += [f'{stamp(p["start"])} — {p["heading"]}' for p in parts]
(OUT/'영상_구간안내.txt').write_text('\n'.join(guide)+'\n',encoding='utf-8')
print(f'PASS: {FINAL}\nDuration: {DURATION:.3f}; bytes: {FINAL.stat().st_size}',flush=True)
