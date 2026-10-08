using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    [Serializable] public sealed class PuzzleClearance
    {
        public int rulesVersion = 1;
        public string stageId, fingerprint;
        public int[] clicks = Array.Empty<int>();
    }
    [Serializable] public sealed class PuzzleClearanceFile
    {
        public PuzzleClearance[] puzzles = Array.Empty<PuzzleClearance>();
    }
    [Serializable] public sealed class PuzzleWorkshopDraft
    {
        public int schemaVersion = 1;
        public HotelStage stage;
        public PuzzleClearance clearance;
    }

    // A reproducible winning replay, not a claim about a player's identity.
    public static class HotelPuzzleClearance
    {
        public static HotelStage Clone(HotelStage stage) => JsonUtility.FromJson<HotelStage>(JsonUtility.ToJson(stage));

        public static string Fingerprint(HotelStage stage)
        {
            var canonical = Clone(stage);
            canonical.id = ""; canonical.title = "";
            canonical.obstacles = canonical.obstacles.OrderBy(o => o.y).ThenBy(o => o.x).ToArray();
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("hotel-puzzle-rules-1\n" + JsonUtility.ToJson(canonical)))).Replace("-", "");
        }

        public static PuzzleClearance Certify(HotelStage stage, IEnumerable<int> clicks)
        {
            var proof = new PuzzleClearance { stageId = stage.id, fingerprint = Fingerprint(stage), clicks = clicks.ToArray() };
            Verify(stage, proof);
            return proof;
        }

        public static void Verify(HotelStage stage, PuzzleClearance proof)
        {
            ContentValidation.ValidateStage(stage);
            ContentValidation.Require(stage.fixedSeed, "검증 퍼즐은 재도전 시에도 같은 시드를 사용해야 합니다.");
            ContentValidation.Require(proof != null && proof.rulesVersion == 1 && proof.stageId == stage.id && proof.fingerprint == Fingerprint(stage), "퍼즐이 변경되었습니다. 클리어 검증을 다시 진행하세요.");
            ContentValidation.Require(proof.clicks != null && proof.clicks.Length > 0 && proof.clicks.Length <= stage.moves, "클리어 기록의 이동 횟수가 올바르지 않습니다.");
            var game = new HotelPuzzle(stage);
            foreach (int index in proof.clicks)
            {
                ContentValidation.Require(!game.Cleared && !game.Failed && index >= 0 && index < stage.width * stage.height, "클리어 기록에 잘못된 클릭이 있습니다.");
                int remaining = game.Session.RemainingMoves;
                game.Play(new GridPosition(index % stage.width, stage.height - 1 - index / stage.width));
                ContentValidation.Require(game.Session.RemainingMoves == remaining - 1, "클리어 기록에 유효하지 않은 이동이 있습니다.");
            }
            ContentValidation.Require(game.Cleared, "목표를 모두 달성한 클리어 기록이 필요합니다.");
        }

        public static void VerifyPack(HotelPack patch, PuzzleClearanceFile file)
        {
            ContentValidation.Require(patch?.stages != null && file?.puzzles != null && file.puzzles.Length == patch.stages.Length, "배포하는 모든 퍼즐의 클리어 기록이 필요합니다.");
            ContentValidation.Require(file.puzzles.All(p => p != null) && file.puzzles.Select(p => p.stageId).Distinct().Count() == file.puzzles.Length, "클리어 기록이 중복되거나 비어 있습니다.");
            foreach (var stage in patch.stages) Verify(stage, file.puzzles.SingleOrDefault(p => p.stageId == stage.id));
        }
    }

    public static class HotelWorkshopFiles
    {
        public static void SaveDraft(string path, PuzzleWorkshopDraft draft)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(draft, true), new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }

        public static PuzzleWorkshopDraft LoadDraft(string path, string expectedId)
        {
            var draft = HotelStorage.ReadJson<PuzzleWorkshopDraft>(path);
            ContentValidation.Require(draft != null && draft.schemaVersion == 1 && draft.stage != null && draft.stage.id == expectedId, "이 초안의 형식이나 대상 퍼즐이 올바르지 않습니다.");
            ContentValidation.Require(draft.stage.width >= 3 && draft.stage.width <= 9 && draft.stage.height >= 3 && draft.stage.height <= 9 && draft.stage.layout != null && draft.stage.layout.Length == draft.stage.height && draft.stage.layout.All(r => r != null && r.Length == draft.stage.width && r.All(c => c == 'O' || c == 'X')) && draft.stage.obstacles != null, "초안의 격자 데이터가 올바르지 않습니다.");
            draft.stage.fixedSeed = true;
            if (draft.clearance != null)
            {
                try { HotelPuzzleClearance.Verify(draft.stage, draft.clearance); }
                catch (InvalidOperationException) { draft.clearance = null; }
            }
            return draft;
        }

        public static string Export(HotelStorage storage, PuzzleWorkshopDraft draft)
        {
            HotelPuzzleClearance.Verify(draft.stage, draft.clearance);
            ContentValidation.Require(storage.BasePack.stages.Any(s => s.id == draft.stage.id), "기본 이야기에서 교체할 퍼즐을 선택하세요.");
            string id = "workshop." + Guid.NewGuid().ToString("N");
            string folder = Path.Combine(storage.ModsPath, id);
            string exports = Path.Combine(storage.Root, "Workshop", "Exports");
            Directory.CreateDirectory(exports);
            var patch = new HotelPack { stages = new[] { HotelPuzzleClearance.Clone(draft.stage) } };
            var proofs = new PuzzleClearanceFile { puzzles = new[] { draft.clearance } };
            HotelPuzzleClearance.VerifyPack(patch, proofs);
            Directory.CreateDirectory(folder);
            var manifest = new ModManifest { id = id, name = "제작실 · " + draft.stage.title, version = "1.0.0", author = "퍼즐 제작자", kind = "content", contentFile = "content.json", clearanceFile = "clearance.json", description = "직접 클리어한 퍼즐 · 기본 이야기의 " + draft.stage.id + " 교체 · 별도 저장" };
            File.WriteAllText(Path.Combine(folder, "content.json"), JsonUtility.ToJson(patch, true));
            File.WriteAllText(Path.Combine(folder, "clearance.json"), JsonUtility.ToJson(proofs, true));
            File.WriteAllText(Path.Combine(folder, "manifest.json"), JsonUtility.ToJson(manifest, true));
            File.WriteAllText(Path.Combine(folder, "README.txt"), "퍼즐 제작실에서 만든 콘텐츠 MOD입니다.\n이 폴더를 게임의 MOD 폴더에 넣고 MOD 관리에서 선택하세요.\n새 조사에서는 지정한 기본 퍼즐이 교체됩니다. 퍼즐 연습에서 바로 플레이할 수도 있습니다.\n본편과 다른 저장을 사용합니다.\n검증 퍼즐은 재도전해도 같은 시드로 시작합니다.\n");
            string zip = Path.Combine(exports, id + ".zip");
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                foreach (string file in Directory.GetFiles(folder)) archive.CreateEntryFromFile(file, id + "/" + Path.GetFileName(file));
            storage.RefreshMods();
            ContentValidation.Require(storage.Mods.Any(m => m.Valid && m.manifest.id == id), "내보낸 MOD를 다시 불러오지 못했습니다.");
            return zip;
        }
    }
}
