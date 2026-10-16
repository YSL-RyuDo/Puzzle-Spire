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
OUT = ROOT / 'Builds/Presentations/PlatformFlow-20261014'
FF = Path('C:/Users/GCT/AppData/Local/Programs/Python/Python310/Lib/site-packages/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe')
DOWNLOADS = Path('C:/Users/GCT/Downloads')
PROVIDED = DOWNLOADS / '[게임1]MOD게임 연말 발표용.mp4'
PRIOR = ROOT / 'Builds/Hotel/StoryStudioDemo-20261012/MoonlitHotel-Create-MOD-and-Play-20261012.mp4'
SOURCES = {'studio': ROOT / 'Builds/Hotel/StoryStudioDemo-20261012/MoonlitHotel-FullPlay.mp4', 'test': DOWNLOADS / '시연 테스트.mp4'}
LOGO = DOWNLOADS / 'GCT 센터 로고.png'
FONT = ROOT / 'Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf'
FINAL = OUT / 'GCT_MOD플랫폼_등록검증다운로드_상세본_v3.mp4'
RENDERED = OUT / 'composite-v3-before-timing-normalization.mp4'
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

# Each clip retains its source recording and operation-specific caption.
segments=[]
def add(source,start,end,heading,text,mode='full',cues=None):
    segments.append(dict(source=source,source_start=start,source_end=end,heading=heading,
                         mode=mode,text=text,cues=cues))
add('black',0,8,'GCT 플랫폼 · 에셋과 MOD의 등록부터 적용까지','에셋 등록·검증, 게임·MOD 배포와 게임 내 적용 과정을 연결하여 소개합니다.')
add('test',48,53,'01  서버 구동 · 서비스 준비 로그','서버와 작업 처리 서비스가 준비된 상태를 실행 로그로 확인합니다.')
add('test',83,95,'02  에셋 유입 · 이미지 파일과 기본 정보','Create Assets에서 이미지 파일을 선택하고 이름과 이용 조건을 입력합니다.')
add('test',95,105,'02  에셋 유입 · 설명과 태그','에셋 설명과 태그를 지정하여 콘텐츠 정보를 작성합니다.')
add('test',105,114,'02  에셋 유입 · 저작권 정보와 업로드','저작권 정보와 카테고리를 입력한 뒤 이미지 업로드를 요청합니다.')
add('test',114,121,'03  서버 로그 확대 · 이미지 검증 시작','업로드 요청을 받은 서버가 SHA-256 중복 검사와 기존 워터마크 탐지를 진행합니다.','log')
add('test',121,127,'03  서버 로그 확대 · 보호 처리 결과','검증 결과와 워터마크 보호 작업, 처리 결과의 DB 저장 로그를 확인합니다.','log')
add('test',127,134,'03  서버 로그 확대 · IPFS 저장 결과','IPFS 업로드 결과와 이미지·메타데이터의 CID가 로그에 기록됩니다.','log')
add('test',134,143,'04  플랫폼 · 등록된 에셋 목록','등록한 배경 에셋의 이름, 분류와 작성자 정보를 목록에서 확인합니다.')
add('test',143,149,'04  플랫폼 · 에셋 다운로드 패널','Download Assets에서 등록한 에셋 항목을 확인합니다.')
add('test',174,181,'05  검증 사례 · 동일 이미지 재업로드','동일한 이미지를 다시 업로드하여 중복 검사가 동작하는지 확인합니다.')
add('test',181,186,'05  서버 로그 확대 · 중복 이미지 차단','기존 이미지와 100% 동일한 것으로 판정되어 후속 IPFS 업로드가 차단됩니다.','log')
add('test',186,190,'05  검증 사례 · 화면에 오류 사유 표시','중복 이미지라는 처리 결과가 플랫폼 화면에도 표시됩니다.')
add('test',204,214,'06  원본 게임 등록 · 게임 정보 입력','개발자 센터에서 게임 번호, 이름, 소개, 버전과 설치 안내를 등록합니다.')
add('test',214,225,'06  원본 게임 등록 · 실행 파일 업로드와 공개','게임 배포 ZIP을 업로드하고 공개하여 창작마당에 연결합니다.')
add('test',225,238,'07  원본 게임 다운로드 · 이용자 화면','상세 페이지의 원본 게임 다운로드를 실행하고 브라우저의 수신 결과를 확인합니다.')
add('test',238,248,'08  MOD 등록 · 원본 게임과 연결','원본 게임을 선택하고 MOD 이름, 변경 내용, 버전과 적용 안내를 등록합니다.')
add('test',248,253,'08  MOD 등록 · ZIP 패키지 업로드','별도로 만든 MOD ZIP을 업로드하여 서버 검증을 요청합니다.')
add('test',249,257,'09  서버 로그 확대 · MOD 내부 에셋 검증','MOD 내부의 캐릭터·배경·수집품 이미지 등 파일별 검증 결과가 기록됩니다.','log',[(0,4,'MOD 내부의 캐릭터·배경·수집품 이미지 등 파일별 검증 결과가 기록됩니다.'),(4,8,'검사 결과는 정상 9개, 중복·충돌 0개로 기록되며 플랫폼 검증을 통과합니다.')])
add('test',257,263,'10  MOD ZIP 받기 · 등록 파일 다운로드','검증 결과를 확인한 뒤 내 등록 ZIP 받기로 MOD 파일을 내려받습니다.')
add('test',260,264,'10  서버 로그 확대 · MOD 파일 전송','MOD 원본 ZIP 요청에 대한 HTTP 200 응답과 전송 크기를 확인합니다.','log')
add('test',267,284,'11  설치 안내와 다운로드 기록','게임의 설치·MOD 적용 안내와 다운로드 기록을 확인합니다.',cues=[(0,10,'게임 ZIP과 MOD ZIP의 설치·적용 안내를 상세 페이지에서 확인합니다.'),(10,17,'내 다운로드에서 원본 게임 다운로드 내역을 확인합니다.')])
add('test',295,302,'12  서버 로그 확대 · 중복 MOD ZIP 차단','동일한 MOD ZIP의 재등록은 SHA-256 비교로 차단되며 HTTP 409가 기록됩니다.','log')
add('test',304,310,'12  검증 사례 · 중복 MOD 등록 차단 안내','플랫폼 화면에서도 동일한 ZIP의 중복 등록이 차단되었음을 안내합니다.')
add('black',0,8,'13  다운로드 이후 · 게임에서 MOD 적용','등록·다운로드한 MOD의 활용 사례로 게임 내 선택과 변경 결과를 보여줍니다.')
add('test',311,320,'13  적용 전 · 기본 대화 인물','그림 MOD를 변경하기 전의 대화 인물 모습을 확인합니다.')
add('test',537,545,'13  적용 전 · 배경과 퍼즐 블록','적용 전 배경과 퍼즐 블록, 목표와 남은 횟수를 확인합니다.')
add('test',549,566,'14  MOD 적용 · 테마 선택과 이어하기','MOD 관리에서 설치된 테마를 선택하고 기존 게임을 이어갑니다.',cues=[(0,7,'타이틀에서 MOD 관리로 들어가 설치된 MOD 목록을 확인합니다.'),(7,17,'사용할 테마를 선택한 뒤 돌아가기와 계속하기로 변경을 적용합니다.')])
add('test',566,580,'15  적용 후 · 배경과 퍼즐 블록','퍼즐 배치와 목표는 유지되고 배경과 블록 이미지가 바뀝니다.')
add('test',818,827,'15  적용 후 · 대화 인물','대화 장면의 인물 이미지와 자세에도 변경한 테마가 반영됩니다.')
add('test',840,846,'15  적용 후 · 수집품','수집품 목록과 이미지도 MOD 교체 대상으로 사용할 수 있습니다.')
add('studio',8,15,'16  콘텐츠 제작 요약 · 배경 가져오기','새 이야기의 장소를 추가하고 배경 이미지를 불러옵니다.')
add('studio',36,42,'16  콘텐츠 제작 요약 · 대사 작성','조사에 필요한 대사와 화자를 편집합니다.')
add('studio',64,69,'16  콘텐츠 제작 요약 · 퍼즐 편집','퍼즐의 보드 배치와 목표를 편집합니다.')
add('studio',73,81,'16  콘텐츠 제작 요약 · 제작자 클리어 검증','제작자가 직접 퍼즐을 완료하여 배포 전 클리어 검증을 수행합니다.')
add('studio',109,117,'16  콘텐츠 제작 요약 · MOD 패키지 내보내기','제작한 이야기를 로컬 MOD와 공유용 ZIP으로 내보냅니다.')
add('studio',119,131,'17  확장 사례 · 독립 이야기 MOD 시작','새 이야기 MOD를 선택하고 새 게임을 시작합니다.')
add('studio',132,138,'17  확장 사례 · 작성한 대사 적용','편집한 대사와 인물 구성이 실제 플레이에 적용됩니다.')
add('studio',148,155,'17  확장 사례 · 제작한 퍼즐 적용','제작한 퍼즐 보드와 목표를 불러와 플레이합니다.')
add('studio',176,181,'17  확장 사례 · 수수께끼 적용','직접 작성한 수수께끼와 정답 처리를 확인합니다.')
add('studio',188,200,'17  확장 사례 · 마지막 대화와 엔딩','마지막 대화를 진행하면 작성한 엔딩 제목과 내용이 표시됩니다.')
add('black',0,8,'플랫폼 흐름 정리','에셋 유입과 서버 검증, 게임·MOD 등록·다운로드, 게임 내 적용을 시연했습니다.')
add('black',0,20,'사사','')
parts=[];frame=0;captions=[]
for i,s in enumerate(segments):
    frames=round((s['source_end']-s['source_start'])*FPS)
    p=dict(s,index=i,frames=frames,start=frame/FPS,end=(frame+frames)/FPS,file=f'parts/cfr-{i:02d}.mp4')
    parts.append(p);frame+=frames
    if s['cues']:
        for a,b,t in s['cues']: captions.append((p['start']+a,p['start']+b,t))
    elif s['text']: captions.append((p['start'],p['end'],s['text']))
DURATION=frame/FPS;ACK_START=parts[-1]['start']
for a,b,t in captions:
    assert b>a and ImageFont.truetype(str(FONT),34).getlength(t)<1730,t
for x,y in zip(captions,captions[1:]): assert abs(x[1]-y[0])<.001,(x,y)
assert captions[0][0]==0 and captions[-1][1]==ACK_START
manifest=dict(duration=DURATION,fps=FPS,size=[1920,1080],content_rect=[160,46,1600,900],
              project_caption=PROJECT,acknowledgement=ACK,watermark=dict(opacity=.15,angle=-20,repeated=True),
              supplied_studio_sha256=sha(PROVIDED),clean_original_sha256=sha(SOURCES['studio']),
              sources={k:str(v) for k,v in SOURCES.items()},parts=parts)
(OUT/'edit-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

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
def card(index,lines):
    p=parts[index]
    for text,y,size in lines: event(p['start'],p['end'],'Card',text,rf'\pos(960,{y})\fs{size}')
card(0,[('GCT 플랫폼 · 등록에서 게임 적용까지',355,58),
        ('에셋 유입 · 서버 검증 · 게임과 MOD 등록 · 다운로드',465,36),
        ('실제 화면과 처리 로그를 연결한 시연',580,34)])
card(24,[('다운로드한 MOD를 게임에 적용',320,54),
         ('설치 안내 확인 → MOD 폴더에 압축 해제 → 목록 새로고침',440,34),
         ('MOD 선택 → 돌아가기 → 계속하기',525,36),
         ('캐릭터 · 배경 · 퍼즐 블록 · 수집품 이미지 교체',630,34)])
card(41,[('플랫폼을 중심으로 이어지는 콘텐츠 흐름',315,48),
         ('에셋 업로드 → 서버 검증과 처리 기록',430,36),
         ('원본 게임·MOD 분리 등록 → 다운로드',510,36),
         ('게임 내 선택·적용 → 그림과 이야기 확장',590,36)])
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
    if p['mode']=='log':
        vf='crop=754:438:0:642,scale=-2:900:flags=lanczos,pad=1600:900:(ow-iw)/2:0:color=black'
    elif p['source']=='test' and p['source_start']<311:
        # Keep the actual server console visible. Mask only the Windows taskbar to its right.
        vf='drawbox=x=754:y=1033:w=1166:h=47:color=black:t=fill,scale=1600:900:flags=lanczos'
    cmd+=['-map','0:v:0','-vf',vf+',setsar=1,fps=24,setpts=N/(24*TB),setparams=range=limited:color_primaries=bt709:color_trc=bt709:colorspace=bt709',
          '-frames:v',str(p['frames']),'-an','-map_metadata','-1','-map_chapters','-1','-c:v','libx264','-preset','fast','-crf','16',
          '-bf','0','-r','24','-fps_mode','cfr','-video_track_timescale','24000',
          '-pix_fmt','yuv420p','-threads','4',str(target)]
    subprocess.run(cmd,check=True,stderr=subprocess.PIPE)
    return p['index']

with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    for i in pool.map(prepare,parts): print(f'Prepared segment {i+1}/{len(parts)}',flush=True)
# The intermediate MP4 muxer reports each segment one frame short. Explicit
# durations prevent that rounding from accumulating against the caption clock.
(OUT/'concat.txt').write_text('\n'.join("file '"+p['file']+"'\nduration "+f'{p["frames"]/FPS:.9f}' for p in parts)+'\n',encoding='utf-8')
if args.prepare_only:
    print(f'Prepared {DURATION:.3f} seconds. Acknowledgement begins {ACK_START:.3f}.',flush=True)
    raise SystemExit(0)

positions=[(x,y) for row,y in enumerate((-60,275,610)) for x in ((-150,390,930,1470) if row%2==0 else (-390,150,690,1230))]
graph=['[0:v]fps=24,setpts=N/(24*TB),format=rgba[scene]',
       '[1:v]format=rgba,colorkey=0xFFFFFF:0.12:0.04,scale=440:-1,'
       'colorchannelmixer=rr=0:rg=0:rb=0:ra=1:gr=0:gg=0:gb=0:ga=1:br=0:bg=0:bb=0:ba=1:aa=0.15,'
       'rotate=-20*PI/180:ow=rotw(-20*PI/180):oh=roth(-20*PI/180):c=none,'
       'split=12'+''.join(f'[logo{i}]' for i in range(12))]
previous='scene'
wm_enable='+'.join(f'gte(t,{p["start"]})*lt(t,{p["end"]})' for p in parts if p['source']!='black')
for i,(x,y) in enumerate(positions):
    current=f'wm{i}';graph.append(f'[{previous}][logo{i}]overlay=x={x}:y={y}:format=auto:enable=\'{wm_enable}\'[{current}]');previous=current
card_enable='+'.join(f'gte(t,{p["start"]})*lt(t,{p["end"]})' for p in parts if p['source']=='black')
graph.append(f'[{previous}]drawbox=x=0:y=0:w=iw:h=ih:color=black:t=fill:enable=\'{card_enable}\','
             'pad=1920:1080:160:46:color=black,'
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
                                  '-pix_fmt','yuv420p','-threads','6','-progress','render-progress.txt','-movflags','+faststart',str(RENDERED)],cwd=OUT,check=True,stderr=log)
# Normalize concat/overlay timestamps and remove the uncaptioned fractional tail.
subprocess.run([str(FF),'-v','error','-y','-i',str(RENDERED),'-map','0:v:0','-map_metadata','0','-map_chapters','0',
                '-vf','fps=24','-t',str(DURATION),'-frames:v',str(frame),'-an','-c:v','libx264','-preset','fast','-crf','18',
                '-pix_fmt','yuv420p','-threads','6','-movflags','+faststart',str(FINAL)],check=True)
print('Render complete; decoding the entire file.',flush=True)
result=subprocess.run([str(FF),'-v','error','-xerror','-i',str(FINAL),'-map','0:v:0','-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE)
(OUT/'verification.txt').write_text('PASS: complete final video decoded without errors.\n'
    +f'Duration: {DURATION:.3f} seconds\nResolution: 1920x1080\nFPS: 24\n'
    +'Watermark: repeated diagonal GCT logo, alpha 0.15, applied once from clean originals.\n'
    +'Project caption: exact requested text; visible from first frame to end.\n'
    +'Acknowledgement: user-approved Korean institution spelling; all other requested wording retained.\n'
    +'Footage is edited, not a continuous live capture. Platform UI and actual server console retained, with labelled log closeups.\n'
    +result.stderr.decode(errors='replace'),encoding='utf-8')
guide=['GCT MOD 플랫폼 등록·검증·다운로드 상세본 v3',f'길이: {stamp(DURATION)} / 1920×1080 / 24fps / 무음',
       '플랫폼과 서버 처리 로그를 중심으로 재편집했습니다. 제작실 편집은 34초로 축약했습니다.',
       '이미지 SHA-256 중복 검사, 워터마크 처리, DB 기록, IPFS 결과, MOD 파일 검증과 중복 ZIP 차단을 포함합니다.',
       'MOD 다운로드 화면은 제작자 본인의 내 등록 ZIP 받기입니다. MONACT 공개 배포 완료로 표현하지 않았습니다.',
       '에셋 썸네일 미표시 등 원본 화면 상태는 그대로 두었습니다. 검사 결과는 촬영된 시연 결과입니다.',
       '제작실의 신규 이야기 ZIP 내보내기는 로컬 시연이며, 해당 ZIP의 플랫폼 업로드를 촬영한 것은 아닙니다.',
       '대각선 반복 GCT 워터마크 15%, 하단 기능 설명, 맨 아랫줄 과제명 상시 표기.',
       '기존 20% 워터마크 파일과 동일한 영상임을 해시로 확인한 뒤 보관된 무워터마크 원본을 사용했습니다.',
       '마지막 검정 사사 화면은 20초이며, 기관명은 사용자 확인에 따라 한국콘텐츠진흥원으로 수정했습니다.',
       '',PROJECT,'',ACK,'','구간 안내:']
guide += [f'{stamp(p["start"])} — {p["heading"]}' for p in parts]
(OUT/'영상_구간안내.txt').write_text('\n'.join(guide)+'\n',encoding='utf-8')
print(f'PASS: {FINAL}\nDuration: {DURATION:.3f}; bytes: {FINAL.stat().st_size}',flush=True)
