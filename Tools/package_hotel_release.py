"""Package only Windows runtime files; theme MOD is a separate, installable ZIP."""
import argparse
import json
import tempfile
import uuid
import zipfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--suffix', default='', help='Optional new package filename suffix')
args = parser.parse_args()
assert all(c.isalnum() or c in '-_' for c in args.suffix), 'Invalid filename suffix'
suffix = '-' + args.suffix if args.suffix else ''
project = Path(__file__).resolve().parents[1]
build = project / 'Builds/Hotel'
output = project / 'Builds/Distribution'
output.mkdir(parents=True, exist_ok=True)
theme = project / 'Assets/StreamingAssets/Hotel/Themes/ClassicIllustrated'
manifest = json.loads((theme / 'manifest.json').read_text(encoding='utf-8-sig'))
game_zip = output / f'MoonlitHotel-Windows{suffix}.zip'
mod_zip = output / f'MoonlitHotel-ClassicIllustrated-MOD-{manifest["version"]}{suffix}.zip'
runtime_files = ['MoonlitHotel.exe', 'UnityPlayer.dll', 'UnityCrashHandler64.exe']
runtime_folders = ['MoonlitHotel_Data', 'MonoBleedingEdge', 'D3D12', 'ThirdPartyNotices']
for required in runtime_files + ['MoonlitHotel_Data', 'MonoBleedingEdge']:
    assert (build / required).exists(), f'Missing runtime: {required}'
files = [build / name for name in runtime_files]
if (build / 'WinPixEventRuntime.dll').exists():
    files.append(build / 'WinPixEventRuntime.dll')
for directory in runtime_folders:
    if (build / directory).is_dir():
        files.extend(p for p in (build / directory).rglob('*') if p.is_file())
excluded_extensions = {'.meta', '.pdb', '.mdb', '.mp4', '.webm', '.log'}
with zipfile.ZipFile(game_zip, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(files):
        relative = path.relative_to(build)
        if path.suffix.lower() in excluded_extensions:
            continue
        if relative.parts[:3] == ('MoonlitHotel_Data', 'StreamingAssets', 'Hotel') and len(relative.parts) > 3 and relative.parts[3] in {'Themes', 'ExampleMod'}:
            continue
        if any('DoNotShip' in part for part in relative.parts):
            continue
        archive.write(path, (Path('MoonlitHotel') / relative).as_posix())
with zipfile.ZipFile(mod_zip, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(theme.rglob('*')):
        if path.is_file() and path.suffix.lower() != '.meta':
            archive.write(path, (Path('ClassicIllustrated') / path.relative_to(theme)).as_posix())

verification = Path(tempfile.gettempdir()) / ('PuzzleSpire-PackageVerify-' + uuid.uuid4().hex)
qa_root = verification / 'UserData'
for path, destination in [(game_zip, verification), (mod_zip, qa_root / 'Mods')]:
    with zipfile.ZipFile(path) as archive:
        assert archive.testzip() is None, f'Corrupted ZIP: {path}'
        for name in archive.namelist():
            assert '..' not in Path(name).parts and not Path(name).is_absolute()
        archive.extractall(destination)
with zipfile.ZipFile(game_zip) as archive:
    names = archive.namelist()
    assert not any(Path(name).suffix.lower() in excluded_extensions for name in names)
    assert not any('/Themes/' in name or '/ExampleMod/' in name for name in names)
    assert not any(part in {'Verification', 'Checkpoints', 'FullPlay', 'ModFullPlay', 'ThemeReview', 'Smoke'} for name in names for part in Path(name).parts)
    assert 'MoonlitHotel/MoonlitHotel_Data/StreamingAssets/Hotel/base.json' in names
with zipfile.ZipFile(mod_zip) as archive:
    names = set(archive.namelist())
    assert 'ClassicIllustrated/manifest.json' in names
    for item in manifest['images']:
        assert 'ClassicIllustrated/' + item['file'] in names, item['key']
report = {
    'game_zip': str(game_zip), 'game_bytes': game_zip.stat().st_size,
    'mod_zip': str(mod_zip), 'mod_bytes': mod_zip.stat().st_size,
    'game_exe': str(verification / 'MoonlitHotel/MoonlitHotel.exe'),
    'qa_root': str(qa_root), 'mod_image_keys': len(manifest['images']),
    'archive_checks': 'PASS: CRC, runtime allowlist, no recordings/debug/source files, no bundled MODs; separate MOD has every referenced image'
}
(output / f'package-verification{suffix}.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=False, indent=2))
