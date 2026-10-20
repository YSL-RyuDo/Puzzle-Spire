# Moonlit Hotel

<!-- portfolio-languages -->
[![English](https://ysl-ryudo.github.io/assets/diagrams/language-en.svg)](README.md) [![한국어](https://ysl-ryudo.github.io/assets/diagrams/language-ko.svg)](README.ko.md) [![日本語（選択中）](https://ysl-ryudo.github.io/assets/diagrams/language-ja-current.svg)](README.ja.md)
<!-- /portfolio-languages -->

調査・会話・パズルを組み合わせ、ローカルストーリー制作に対応するミステリーアドベンチャーです。

| プロジェクト情報 | 内容 |
|---|---|
| 開発状況 | プレイ可能なプロトタイプ |
| 使用技術 | C#, Unity, JSON / PNG |

[開発事例を見る](https://ysl-ryudo.github.io/ja/work/moonlit/), [Portfolio](https://ysl-ryudo.github.io/ja/)

## ゲーム内の実装

[![有効な交換、目標数、残り手数が連動するマッチ3パズル。](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)

有効な交換、目標数、残り手数が連動するマッチ3パズル。

[![ストーリーノードの順序に沿って会話と人物表示が進行します。](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)

ストーリーノードの順序に沿って会話と人物表示が進行します。

[プレイ動画を見る](https://ysl-ryudo.github.io/ja/work/moonlit/#gameplay), 最初の調査、会話、パズル、手がかり収集、保存後の再開、40秒。2026年10月20日にゲーム内の自動入力で収録。

## ゲーム全体で担当した実装

Moonlit Hotelは調査・物語進行、マッチパズル規則、利用者向け制作ツールを含みます。ResolverとMOD形式に加え、キャンペーン・手掛かり収集・パズル表示・物語編集・試験を実装しました。

[![担当実装の流れ](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-ja.svg)](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-ja.svg)

### 調査と物語進行

新規・継続プレイ、場所・ノード進行、会話・なぞ・結末を実装しました。調査対象と手帳・収集項目を記録し、ワールド操作を蓄積した手掛かりにつなぎます。

[HotelGame.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelGame.cs), [担当変更 65c3791](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/65c379100887602f8a06cbcfb8edf00d0d512ea6), [HotelInvestigation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelInvestigation.cs), [担当変更 26d8094](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/26d8094e20e400818c2576b1406bdb24f4adf7cf)

### パズル規則とターン演出

マッチ・特殊ブロック・障害物・目標の規則層と、移動・除去・効果の表示層を実装しました。表示コールバックだけで規則を決めず、解決結果に従ってターンを表示します。

[HotelPuzzle.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Gameplay/HotelPuzzle.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelPuzzlePresentation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzlePresentation.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Story Studioの物語制作

物語ノード・選択肢編集、調査・なぞのフォーム、プレビューを実装しました。プロジェクトを検証しコンテンツパックとして出力できます。毎回コードを書き換えず、制作と試行を製品内でつなぎます。

[HotelStoryStudioEditing.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelStoryStudioEditing.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Puzzle Workshopとコンテンツ保存

Puzzle Workshopで盤面編集、プロジェクト保存・読込、試遊、出力検証を実装しました。外観テーマと内容変更の保存境界を分け、外観変更だけで進行が不必要に分離しないようにしました。

[HotelPuzzleWorkshop.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzleWorkshop.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [担当変更 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

## 貢献の根拠

[Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

外部素材・チームメンバーの貢献は区別します。

## 実装と設計

推理アドベンチャーの調査・会話・パズルを連携し、新しい物語の制作・読み込み時に本編と保存状態が混ざらないよう構成しました。

パズルのドメインルールと表示を分離し、クリア検証、物語の編集・書き出し、ローカルMODごとの独立した保存状態を実装しました。

## コードの動作と実装構造の理由

### パズル規則を画面演出から分ける理由

**実装の流れ**

PuzzleTurnResolverは盤面データに対して連結ブロック探索、削除、重力、生成、特殊ブロック規則を明示的な依存関係で組み合わせます。選択処理前に状態機械の入力可否を確認し、ターン結果を返します。

**この構造が必要な理由**

規則をアニメーション完了コールバックだけに置くと、シーンなしで状態変化を確認しにくくなります。C#のドメイン層は表示なしで盤面遷移を検査でき、発生した事実と見せ方を分離します。入力状態の確認もターン開始条件を明確にします。

**代案と現在の実装の折合い**

小さな試作ではMonoBehaviourコールバックに規則を置く方が接続コードは少なくなります。ドメイン分離は接続作業を増やしますが、全シーン条件を再現せず規則変更と検査ができます。現在のResolverはこの分離を使い、表示層は結果を正しく反映する必要があります。

**利点・代償と検証範囲**

分離に伴いインターフェースと結果データが増えます。入力・アニメーション・次ターンが完了時点を共有するよう、規則の結果と演出を同期する必要があります。

[Puzzle-Spire/PuzzleTurnResolver.cs](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Assets/Game/Domain/Puzzle/Rules/PuzzleTurnResolver.cs)

### 外観変更とコンテンツ変更で保存境界を分ける理由

**実装の流れ**

テーマMODはアートを差し替えつつ基本セーブを共有し、コンテンツMODはID・バージョンによる別の保存境界を使います。テーマのコンテンツファイル指定は拒否し、パス、領域、画像制限を検査して、不足する素材には基本素材を使います。

**この構造が必要な理由**

外観だけの変更で進行を捨てる必要はありませんが、内容が変わると既存進行と互換でない場合があります。形式で区別すると、含まれるファイルから推測せず、エディターと実行環境で共通の保存規則を適用できます。

**代案と現在の実装の折合い**

すべてのMODで保存を分けると外観変更だけでも進行が分離します。全部共有すると異なる内容が同じ進行を別に解釈し得ます。テーマとコンテンツの形式を分ければ違いを明示し、内容変更に版管理を集中できます。

**利点・代償と検証範囲**

同時に1つのMODだけを適用し、上書き優先順位の衝突を減らす一方、組合せを制限します。内容変更には保存互換性と版管理の規則も必要です。

[Puzzle-Spire/ThemeMods.md](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Docs/ThemeMods.md)

## 実行・コードの確認

[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)記載のUnity版で開いてください。パッケージロックとシリアライズ済み素材を維持し、Unityを開く前に対象ブランチを選んでください。

Unity scene: [Assets/Game/Scenes/Hotel.unity](Assets/Game/Scenes/Hotel.unity)

## 開発・検証範囲

プレイ可能なプロトタイプです。既存READMEの2026 ~ 10 ~ 08記録にはEditMode 141件成功があり、その後の物語MODの通し検証もあります。合法手による自動進行・UI検査は人間の難易度・面白さ評価を代替しません。制作室からのプラットフォーム直接アップロードは未実装で、外部ZIPはMODフォルダーに展開します。

## 詳細資料

[既存の詳細開発・利用・検証資料（韓国語）](docs/DEVELOPMENT_REFERENCE.ko.md)

[Docs/FullPlayVerification.md](Docs/FullPlayVerification.md)

[Docs/HotelPrototype.md](Docs/HotelPrototype.md)

[Docs/ThemeMods.md](Docs/ThemeMods.md)

## 関連リポジトリ

[Puzzle-Spire](https://github.com/YSL-RyuDo/Puzzle-Spire), 公開リポジトリ
