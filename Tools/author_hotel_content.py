"""Rebuild the original Korean demo content. No external packages required."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'Assets/StreamingAssets/Hotel'
def line(who, text): return dict(speaker=who, text=text)
characters = [
 dict(id='you',name='서윤',role='우연히 사건을 맡은 여행자',color='#D9B978'),
 dict(id='front',name='하린',role='기억력이 좋은 프런트 직원',color='#71BDB4'),
 dict(id='keeper',name='도식',role='호텔의 물건을 돌보는 관리 직원',color='#D59C78'),
 dict(id='guest',name='문 선생',role='오랫동안 머문 투숙객',color='#B1A4C7'),
]
locations = [
 dict(id='lobby',name='호텔 로비',subtitle='비가 그친 뒤, 오래된 시계가 다시 움직인다.',unlockAfter='',image='background.lobby'),
 dict(id='corridor',name='3층 복도',subtitle='불 꺼진 객실 사이로 전화벨이 울린다.',unlockAfter='register',image='background.corridor'),
 dict(id='archive',name='기록 보관실',subtitle='잊힌 방에도 누군가의 이름이 남아 있다.',unlockAfter='phone_riddle',image='background.archive'),
]
clues = [
 dict(id='handover',title='세탁물 인계표',description='17:00 흰 수건 인계. 17:10 파란 바구니 복도 이동. 17:20 빈 카트 반환. 인계표 모서리에 작은 황동 자국이 있다.'),
 dict(id='basket',title='파란 바구니의 흔적',description='파란 천 틈에서 303호 열쇠가 나왔다. 바구니 바닥에는 객실 번호 대신 ‘달’이 새겨진 태그도 있었다.'),
 dict(id='moon_tag',title='달 모양 객실 태그',description='앞면에는 초승달, 뒷면에는 ‘사람을 기다리는 방’이라는 글귀가 새겨져 있다. 현재 객실 안내에는 없는 표식이다.'),
 dict(id='phone_log',title='시험 통화 기록',description='매일 18:00, 관리실에서 3층 빈 객실로 시험 신호가 전송된다. 수화기를 들면 즉시 끊긴다.'),
 dict(id='voice_note',title='전화기 밑의 메모',description='‘전시를 다시 여는 날, 달의 방에도 불을 켜 주세요.’ 누군가 오래전에 남긴 부탁이다.'),
 dict(id='old_plan',title='옛 호텔 안내도',description='3층 끝의 방은 객실이 아니라 작은 공동 서재였다. 투숙객과 직원이 편지와 그림을 나누던 곳이다.'),
 dict(id='photo',title='서재 앞에서 찍은 사진',description='젊은 문 선생과 직원들이 초승달 표식 아래 서 있다. 뒤에는 ‘수리 후에도 약속은 남습니다’라고 쓰여 있다.'),
 dict(id='letter',title='부치지 못한 초대장',description='서재를 다시 열면 옛 친구들을 초대하려던 문 선생의 편지. 호텔의 폐업이 아니라 수리 때문에 방이 닫혔다는 설명이 적혀 있다.'),
]

stages=[]
titles=['열쇠 보관함','인계 기록','바구니 주변','남은 물건','전화 교환표','복도의 흔적','전화기 받침','보관실 입구','안내도 서랍','사진 보관함','편지 묶음','서재의 마지막 상자']
for i,title in enumerate(titles,1):
 layout=['OOOOOOO']*7
 if i in (4,7,10): layout=['XOOOOOX']+['OOOOOOO']*5+['XOOOOOX']
 obstacles=[]
 if i in (3,4,8,9,12): obstacles=[dict(x=x,y=2,type='box',durability=1) for x in (2,4)]
 if i in (6,7,10,11,12): obstacles += [dict(x=x,y=4,type='ice',durability=1) for x in (1,3,5)]
 stages.append(dict(id=f'stage_{i:02}',title=title,width=7,height=7,moves=24 if i<5 else 28,seed=1049+i*137,colors=4,targetColor=(i-1)%4,targetAmount=12 if i<3 else 18,boxes=sum(o['type']=='box' for o in obstacles),ice=sum(o['type']=='ice' for o in obstacles),layout=layout,obstacles=obstacles))

riddles=[
 dict(id='route',title='세 번의 이동',kind='order',question='열쇠는 수건에 걸려 함께 이동했다. 인계표에 따르면 수건을 받은 뒤 파란 바구니가 복도로 나갔고, 마지막에 빈 카트가 돌아왔다. 실제 순서대로 기록을 선택하자.',options=['빈 카트가 프런트로 돌아왔다','흰 수건을 인계했다','파란 바구니를 복도로 옮겼다'],answer=[1,2,0],hints=['가장 먼저 일어난 일은 수건을 받은 것이다.','복도로 이동한 것은 카트가 아니라 파란 바구니다.','흰 수건 → 파란 바구니 → 빈 카트.'],explanation='열쇠가 수건에 걸려 있었다면 복도로 나간 파란 바구니를 찾아야 한다. 빈 카트만 살펴봐서는 찾을 수 없다.'),
 dict(id='phone',title='빈 방에서 걸려 온 전화?',kind='choice',question='전화는 매일 정확히 18시에 울린다. 빈 객실에는 아무도 없고, 수화기를 들면 끊긴다. 관리실 기록에는 같은 시각 그 객실로 ‘시험 신호’를 보낸다고 적혀 있다. 가장 잘 맞는 설명은?',options=['빈 객실의 투숙객이 장난 전화를 건다','관리실의 자동 시험 신호가 객실 전화벨을 울린다','옆 객실의 시계가 전화 소리를 낸다'],answer=[1],hints=['소리가 나는 장소와 신호를 보내는 장소는 다를 수 있다.','통화 기록의 방향은 관리실에서 객실로 향한다.','누군가의 통화가 아니라 자동으로 보내는 시험 신호다.'],explanation='전화벨은 사람이 발신했다는 증거가 아니다. 오래된 설비가 매일 같은 시간 연결 상태를 시험하고 있었다.'),
 dict(id='room',title='번호 없는 방',kind='choice',question='달 태그에는 객실 번호가 없다. 옛 안내도에는 같은 표식 옆에 ‘누구나 편지와 책을 남길 수 있는 방’이라고 적혀 있다. 이 방의 원래 용도는?',options=['특정 손님만 쓰는 비밀 객실','직원의 개인 창고','투숙객과 직원이 함께 쓰는 서재'],answer=[2],hints=['특정 사람에게 배정하는 방이었다면 객실 번호가 필요했을 것이다.','핵심은 ‘누구나’라는 표현이다.','책과 편지를 함께 나누던 공간을 고르자.'],explanation='객실 번호가 지워진 것이 아니었다. 처음부터 모두가 이용하는 공동 서재여서 달 표식만 붙어 있었다.'),
 dict(id='timeline',title='닫힌 방의 진짜 이야기',kind='order',question='사진에는 서재가 문을 연 날이 남아 있다. 그 후 누수가 생겨 수리 때문에 문을 닫았다. 편지는 수리가 끝나면 친구들을 초대하려고 썼지만 부치지 못했다. 일어난 순서대로 놓자.',options=['부치지 못한 초대장이 남았다','서재에서 함께 사진을 찍었다','누수 수리로 서재 문을 닫았다'],answer=[1,2,0],hints=['함께 사진을 찍은 때에는 서재를 사용할 수 있었다.','초대장은 다시 열 날을 기다리며 작성했다.','사진 → 수리로 폐쇄 → 부치지 못한 초대장.'],explanation='누군가 방의 존재를 지운 것이 아니었다. 수리가 길어지며 재개장 약속이 잊혔고, 문 선생은 혼자 그 약속을 기다렸다.'),
]

nodes=[]
previous=None
def node(id,location,title,purpose,kind='dialogue',content='',before=(),after=(),rewards=(),chapter='01 · 사라진 303호 열쇠',xy=(.5,.5)):
 global previous
 nodes.append(dict(id=id,chapter=chapter,location=location,title=title,purpose=purpose,kind=kind,contentId=content,x=xy[0],y=xy[1],requires=[previous] if previous else [],rewards=list(rewards),before=[line(*v) for v in before],after=[line(*v) for v in after]))
 previous=id

node('arrival','lobby','프런트의 하린','하린에게 체크인을 요청하자.',before=[('front','잔월 호텔에 오신 걸 환영해요. 서윤 님, 예약 확인했어요. 다만 작은 문제가…….'),('you','제 방이 사라진 건 아니겠죠?'),('front','방은 있어요. 303호 열쇠만 없어요. 아까까지 보관함에 있었는데요.'),('you','그럼 없어진 게 아니라, 어딘가로 옮겨졌겠네요. 마지막으로 본 곳부터 살펴볼게요.')],xy=(.34,.54))
node('keys','lobby','열쇠 보관함','보관함 주변을 정리해 열쇠가 이동한 흔적을 찾자.','stage','stage_01',before=[('you','같은 색 블록이 두 개 이상 붙어 있으면 클릭해서 정리할 수 있어요. 왼쪽 목표를 채워 보죠.'),('front','큰 묶음은 특별한 블록이 돼요. 5개는 로켓, 7개는 폭탄, 9개는 색 제거 블록이에요.')],after=[('you','열쇠 고리가 있던 자리에 흰 실이 걸려 있네요.'),('front','조금 전에 여기서 새 수건을 확인했어요.')],xy=(.58,.39))
node('register','lobby','세탁물 인계 기록','흰 실과 관련된 기록을 확인하자.','stage','stage_02',after=[('front','도식 씨가 수건을 받아 3층으로 올라갔어요. 인계표는 여기 있어요.'),('you','열쇠가 수건에 걸렸을지도 모르겠네요. 물건이 이동한 순서를 확인해 봐요.')],rewards=['handover'],xy=(.43,.68))
node('route_riddle','lobby','인계표의 순서','수건이 어디로 갔는지 기록을 맞춰 보자.','riddle','route',after=[('you','파란 바구니가 마지막 단서예요. 3층 복도로 가 봐야겠어요.')],xy=(.43,.68))
node('keeper','corridor','도식에게 물어보기','복도로 나온 바구니의 위치를 확인하자.',before=[('keeper','아, 파란 바구니요? 수건은 객실에 넣고 바구니는 복도 끝에 뒀어요.'),('you','혹시 작은 황동 열쇠를 보셨나요?'),('keeper','바구니에서 짤랑 소리가 나기는 했죠. 제 허리의 열쇠인 줄 알았는데!')],xy=(.73,.55))
node('basket_search','corridor','파란 바구니','상자를 정리하고 바구니 안쪽을 확인하자.','stage','stage_03',before=[('you','상자는 옆 블록을 없애거나 특수 블록으로 치면 열려요. 목표에 있는 상자도 확인해 주세요.')],after=[('you','수건 사이에 303호 열쇠가 끼어 있어요. 예상대로였네요.'),('keeper','다행이군요. 그런데 바닥에 남은 저 태그는 우리 객실 열쇠가 아닌데요?')],rewards=['basket'],xy=(.62,.76))
node('tag_search','corridor','바구니 바닥','열쇠와 함께 발견된 오래된 물건을 살펴보자.','stage','stage_04',after=[('you','초승달이 새겨진 태그예요. 번호 대신 글귀가 있네요. 사람을 기다리는 방…….'),('guest','그 표식을 아직도 갖고 있었군요. 달의 방이랍니다.'),('you','어느 객실인가요?'),('guest','지금 안내도에는 없을 거예요. 열쇠는 먼저 하린 씨에게 돌려주시죠.')],rewards=['moon_tag'],xy=(.62,.76))
node('key_return','lobby','열쇠 돌려주기','첫 사건을 마무리하고 달의 방을 물어보자.',before=[('front','찾으셨군요! 수건에 열쇠가 따라갔다니, 보관함 위치를 옮겨야겠어요.'),('you','이 달 태그도 같이 있었어요. 혹시 아세요?'),('front','처음 봐요. 그런데 3층 빈 객실에서 매일 전화벨이 울린다는 이야기는 있어요. 오래된 물건과 관련 있을까요?'),('you','소문보다 기록부터 확인해 볼게요.')])
chapter='02 · 빈 객실의 전화'
node('switchboard','lobby','전화 교환 기록','전화벨이 울리는 시각의 기록을 찾자.','stage','stage_05',after=[('front','18시마다 같은 번호로 신호가 나가네요. 발신지는 관리실이에요.'),('you','객실에서 걸려 온 전화라고 했는데, 기록은 반대군요.')],rewards=['phone_log'],chapter=chapter,xy=(.25,.64))
node('hall_trace','corridor','빈 객실 앞','누군가 출입했는지 주변을 확인하자.','stage','stage_06',before=[('you','얼음 표시는 같은 칸의 블록을 없애면 깨져요. 색 목표와 함께 정리해 보죠.')],after=[('keeper','문은 계속 잠겨 있었어요. 저는 복도 전구를 바꾸느라 여기 있었고요.'),('you','사람이 들어갔다는 흔적은 없네요. 전화기 자체를 확인해 볼게요.')],chapter=chapter,xy=(.32,.44))
node('phone_base','corridor','빈 객실의 전화기','전화기 주변을 정리하고 남겨진 기록을 찾자.','stage','stage_07',after=[('you','받침 밑에 메모가 있어요. 달의 방에도 불을 켜 달라고 적혀 있네요.'),('keeper','이 전화는 오래된 교환기에 연결돼 있어요. 자동 점검 기능도 남아 있을 겁니다.')],rewards=['voice_note'],chapter=chapter,xy=(.78,.65))
node('phone_riddle','corridor','전화벨의 정체','관찰한 사실과 기록을 함께 해석하자.','riddle','phone',after=[('you','유령 전화가 아니라 설비 시험이었네요.'),('keeper','내일은 점검 설정을 바꿔 두죠. 달의 방은 보관실의 옛 안내도를 보면 알 수 있을 거예요.')],chapter=chapter,xy=(.78,.65))
chapter='03 · 사람을 기다리는 방'
node('archive_entry','archive','쌓인 보관 상자','안내도가 있는 서랍까지 길을 정리하자.','stage','stage_08',after=[('keeper','여긴 예전 호텔 물건들을 모아 둔 곳이에요. 버리자는 말이 나올 때마다 문 선생이 조금만 더 두자고 하셨죠.')],chapter=chapter,xy=(.28,.72))
node('plan','archive','옛 안내도 서랍','달 표식이 어디에 쓰였는지 찾아보자.','stage','stage_09',after=[('you','이 안내도에도 초승달이 있어요. 누구나 편지와 책을 남길 수 있는 방이라고 적혀 있네요.')],rewards=['old_plan'],chapter=chapter,xy=(.52,.57))
node('room_riddle','archive','달의 방의 용도','태그와 안내도가 가리키는 장소를 알아내자.','riddle','room',after=[('you','객실이 아니라 서재였어요. 번호가 없는 이유도 설명돼요.'),('guest','맞아요. 여행자들이 잠깐씩 자기 이야기를 남기던 곳이었죠.')],chapter=chapter,xy=(.52,.57))
node('photos','archive','사진 보관함','서재를 기억하는 사진을 찾자.','stage','stage_10',after=[('you','사진 속 이분…… 문 선생님이시죠?'),('guest','제가 처음 이 호텔에 머물던 날이에요. 저기 서 있는 친구가 서재를 제안했죠. 언젠가 다시 모이자고 약속했어요.')],rewards=['photo'],chapter=chapter,xy=(.73,.42))
node('letters','archive','묶인 편지','사진 뒤의 약속이 어떻게 되었는지 확인하자.','stage','stage_11',after=[('guest','누수 때문에 문을 닫았어요. 수리가 끝나면 모두를 부르려고 초대장을 썼는데…… 시간이 너무 흘렀군요.'),('you','아직 보내지 않았다는 건, 지금 보낼 수도 있다는 뜻이죠.')],rewards=['letter'],chapter=chapter,xy=(.38,.52))
node('timeline_riddle','archive','남은 기록 연결하기','사진과 편지로 닫힌 방의 사연을 정리하자.','riddle','timeline',after=[('you','사라진 객실이나 숨겨진 손님은 없었어요. 다시 열기로 한 서재와, 그 약속을 기억하는 사람이 있었죠.'),('front','작게라도 다시 열면 어떨까요? 로비 책장을 먼저 옮길 수 있어요.'),('keeper','누수는 오래전에 고쳤습니다. 남은 건 상자 몇 개와 간판뿐이죠.')],chapter=chapter,xy=(.65,.56))
node('last_boxes','archive','서재에 남은 상자','다시 열 날을 위해 마지막 상자를 정리하자.','stage','stage_12',after=[('guest','태그를 제게 주시겠어요? 이번에는 숨겨 두지 않고 문에 걸어 놓을게요.')],chapter=chapter,xy=(.28,.72))
node('ending','lobby','달의 방, 다시 열다','사람을 기다리는 방에 새 약속을 남기자.','ending',before=[('front','303호 열쇠입니다. 체크인이 조금 길어졌네요.'),('you','덕분에 호텔을 제대로 알게 됐어요.'),('guest','서재의 첫 페이지에 이름을 남겨 주겠어요?'),('you','서윤. 열쇠 하나를 찾으러 왔다가, 기다리던 방 하나를 찾음.'),('keeper','다음에는 열쇠를 잃어버리지 않고 초대하겠습니다.'),('front','잔월 호텔의 달의 방은, 오늘부터 다시 열려 있습니다.')],chapter='EPILOGUE · 다시 쓰는 초대장')

# Positions for the PC full-screen illustrated backgrounds.
pc_positions = {'arrival': (0.7, 0.5), 'keys': (0.72, 0.28), 'register': (0.64, 0.43), 'route_riddle': (0.64, 0.43), 'key_return': (0.7, 0.5), 'switchboard': (0.69, 0.44), 'keeper': (0.72, 0.56), 'phone_base': (0.87, 0.56), 'phone_riddle': (0.87, 0.56), 'archive_entry': (0.32, 0.69), 'plan': (0.68, 0.55), 'room_riddle': (0.68, 0.55), 'photos': (0.85, 0.38), 'letters': (0.75, 0.5), 'timeline_riddle': (0.68, 0.55), 'last_boxes': (0.32, 0.69)}
for item in nodes:
 if item["id"] in pc_positions: item["x"], item["y"] = pc_positions[item["id"]]

pack=dict(schemaVersion=1,title='잔월 호텔',locations=locations,characters=characters,clues=clues,nodes=nodes,stages=stages,riddles=riddles)
ROOT.mkdir(parents=True,exist_ok=True)
(ROOT/'base.json').write_text(json.dumps(pack,ensure_ascii=False,indent=2),encoding='utf-8')
sample=ROOT/'ExampleMod'; sample.mkdir(exist_ok=True)
manifest=dict(schemaVersion=1,id='moonlight.example',name='달빛 연습장',version='1.0.0',author='Puzzle Spire',description='첫 스테이지를 여유롭게 바꾸고 새 연습 스테이지와 청록 블록 스킨을 추가하는 예제 MOD.',contentFile='content.json',images=[dict(key='block.0',file='red-block.png')])
easy=dict(stages[0]);easy.update(moves=35,targetAmount=8)
extra=dict(stages[2]);extra.update(id='moonlight_bonus',title='달빛 연습장 · 보너스',moves=30,seed=7781)
(sample/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(sample/'content.json').write_text(json.dumps(dict(schemaVersion=1,stages=[easy,extra]),ensure_ascii=False,indent=2),encoding='utf-8')
# A deterministic original swatch icon; no external imagery is used.
import struct,zlib
w=h=96
rows=[]
for y in range(h):
 row=bytearray()
 for x in range(w):
  border=x<5 or y<5 or x>=w-5 or y>=h-5
  diamond=abs(x-48)+abs(y-48)<22
  row.extend((225,203,142,255) if diamond else ((28,79,86,255) if border else (73,160,163,255)))
 rows.append(b'\0'+row)
def chunk(tag,data):return struct.pack('>I',len(data))+tag+data+struct.pack('>I',zlib.crc32(tag+data)&0xffffffff)
(sample/'red-block.png').write_bytes(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>2I5B',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(b''.join(rows)))+chunk(b'IEND',b''))
print(f'Wrote {len(nodes)} investigations, {len(stages)} stages, {len(riddles)} riddles.')
