"""Apply the first visual investigation without changing node IDs or saved rewards."""
import json
from pathlib import Path


def apply_visual_story(pack):
    nodes = {node['id']: node for node in pack['nodes']}
    nodes['arrival']['before'] = [
        dict(speaker='front', pose='worried', text='서윤 님, 어서 오세요. 303호 열쇠가… 분명 여기 있었는데.'),
        dict(speaker='you', pose='curious', text='비어 있는 자리가 저기인가요?'),
        dict(speaker='front', pose='worried', text='네. 방금 수건을 확인할 때까지는 있었어요.'),
        dict(speaker='you', pose='curious', text='주변에 흔적이 남았을지도 몰라요.'),
    ]
    nodes['keys'].update(
        inspection='cabinet', discovery='thread', discoveryTitle='열쇠 고리의 흰 실',
        discoveryText='빈 303호 고리에 흰 실이 남아 있다.\n수건의 섬유와 닮았다.',
        before=[],
        after=[
            dict(speaker='you', pose='found', text='열쇠 고리에 걸려 있던 실이에요.'),
            dict(speaker='front', pose='remember', text='아, 조금 전 여기 놓았던 수건…!'),
            dict(speaker='you', pose='curious', text='세탁물 인계표도 볼 수 있을까요?'),
        ],
    )
    next(clue for clue in pack['clues'] if clue['id'] == 'basket')['image'] = 'key'
    return pack


if __name__ == '__main__':
    path = Path(__file__).resolve().parents[1] / 'Assets/StreamingAssets/Hotel/base.json'
    pack = json.loads(path.read_text(encoding='utf-8'))
    path.write_text(json.dumps(apply_visual_story(pack), ensure_ascii=False, indent=2), encoding='utf-8')
