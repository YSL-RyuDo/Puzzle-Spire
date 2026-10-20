# Moonlit Hotel

<!-- portfolio-languages -->
[![English (current)](https://ysl-ryudo.github.io/assets/diagrams/language-en-current.svg)](README.md) [![한국어](https://ysl-ryudo.github.io/assets/diagrams/language-ko.svg)](README.ko.md) [![日本語](https://ysl-ryudo.github.io/assets/diagrams/language-ja.svg)](README.ja.md)
<!-- /portfolio-languages -->

A mystery adventure combining investigation, dialogue and puzzles with local story authoring.

| Project | Details |
|---|---|
| Status | Playable prototype |
| Stack | C#, Unity, JSON / PNG |

[Explore the case](https://ysl-ryudo.github.io/en/work/moonlit/), [Portfolio](https://ysl-ryudo.github.io/en/)

## Inside the game

[![The match-three board connects valid swaps, goal counts and the remaining move budget.](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-puzzle.webp)

The match-three board connects valid swaps, goal counts and the remaining move budget.

[![Dialogue progresses through authored story nodes and character presentation.](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)](https://ysl-ryudo.github.io/assets/gameplay/moonlit-dialogue.webp)

Dialogue progresses through authored story nodes and character presentation.

[Watch gameplay](https://ysl-ryudo.github.io/en/work/moonlit/#gameplay), First investigation, dialogue, puzzle, collected clue and save/resume, 0:40. Recorded in-engine with scripted inputs on 20 October 2026.

## What I built across the game

Moonlit Hotel includes an investigation/story game, a match-puzzle domain and player-facing authoring tools. I implemented campaign flow, clue collection, puzzle presentation, story editing and puzzle testing, in addition to the rule resolver and MOD formats.

[![My implementation flow](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-en.svg)](https://ysl-ryudo.github.io/assets/diagrams/moonlit-flow-en.svg)

### Investigation and story progression

Implemented new/continued games, location and node progression, dialogue, riddles and conclusions. Investigation records inspected objects and notebook/collection entries, connecting world interaction to the player's accumulated evidence.

[HotelGame.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelGame.cs), [My change 65c3791](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/65c379100887602f8a06cbcfb8edf00d0d512ea6), [HotelInvestigation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelInvestigation.cs), [My change 26d8094](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/26d8094e20e400818c2576b1406bdb24f4adf7cf)

### Puzzle rules and turn presentation

Implemented a rule layer for matches, special blocks, obstacles and goals, and a presentation layer for movement, removal and effects. The displayed turn follows the resolved rule outcome instead of making visual callbacks the sole source of puzzle rules.

[HotelPuzzle.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Gameplay/HotelPuzzle.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelPuzzlePresentation.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzlePresentation.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Story Studio authoring

Built story-node and choice editing, investigation/riddle forms and preview flows. Story projects can be validated and exported as content packs. This makes editing and trying authored content part of the product rather than requiring a developer to rewrite each story in code.

[HotelStoryStudioEditing.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelStoryStudioEditing.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

### Puzzle Workshop and content saves

Implemented board editing, project load/save, playtesting and export validation in Puzzle Workshop. Theme-only changes and content changes use different save boundaries, so presentation changes need not create unrelated campaign progress.

[HotelPuzzleWorkshop.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Presentation/HotelPuzzleWorkshop.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd), [HotelStoryProject.cs (main)](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/530565b67d90b3673159611170ddbaba4bb79af1/Assets/Game/Hotel/Content/HotelStoryProject.cs), [My change 8505173](https://github.com/YSL-RyuDo/Puzzle-Spire/commit/8505173364c11e98730b2a61ad8503cb52bc9ccd)

## Contribution evidence

[Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

## Implementation and design

A mystery adventure combines investigation, dialogue and puzzles. Authoring and loading new stories should not mix their save state with the base game.

I separated puzzle domain rules from presentation, implemented clear-validation and story editing/export, and gave local MOD stories independent save state.

## How the code works and why it is structured this way

### Puzzle rules do not depend on the animation finishing

**Implementation**

PuzzleTurnResolver works on the board and composes connected-block finding, removal, gravity, spawning and special-block rules through explicit dependencies. It checks whether the state machine accepts input before resolving a selection and returns a turn result.

**Design rationale**

A rule implemented only inside an animation callback is difficult to inspect without running the scene. A C# domain layer makes board transitions testable without scene presentation, and separates the decision about what happened from how it is shown. The input-state check gives turn entry a single explicit rule.

**Alternatives and the current tradeoff**

Putting rules in MonoBehaviour callbacks minimizes boundaries in a small prototype. A separate domain layer needs more plumbing but allows rule changes and tests without reproducing every scene condition. The current resolver uses that separation; presentation still has to consume its results correctly.

**Tradeoffs and verification**

The separation adds interfaces and result data. Rule outcomes and presentation timing must be coordinated so input, animation and the next turn agree on when resolution is complete.

[Puzzle-Spire/PuzzleTurnResolver.cs](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Assets/Game/Domain/Puzzle/Rules/PuzzleTurnResolver.cs)

### Changing appearance should not create a new campaign

**Implementation**

Theme MODs replace art while sharing the base save. Content MODs use their own ID/version save boundary. The theme loader rejects content-file declarations and validates asset paths, regions and image limits; missing replacements fall back to base assets.

**Design rationale**

A cosmetic change should not discard progress, whereas altered content may make that progress incompatible. Distinguishing those cases in the format keeps save semantics explicit for both the editor and runtime instead of inferring them from whichever files happen to be present.

**Alternatives and the current tradeoff**

Isolating every MOD's save avoids sharing but needlessly separates progress for cosmetic changes. Sharing all saves risks incompatible content interpreting the same progress differently. Splitting theme and content formats makes that distinction explicit, with versioning work concentrated on content changes.

**Tradeoffs and verification**

Only one MOD is active at a time, reducing override-precedence conflicts but limiting combinations. Content changes also need save compatibility and versioning rules.

[Puzzle-Spire/ThemeMods.md](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Docs/ThemeMods.md)

## Open and inspect

Open the project with the Unity version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). Keep its package lock and serialized assets intact. Select the documented branch before opening Unity.

Unity scene: [Assets/Game/Scenes/Hotel.unity](Assets/Game/Scenes/Hotel.unity)

## Development and verification scope

Playable prototype. The README's 2026 ~ 10 ~ 08 record reports 141 EditMode tests; subsequent documentation records story-MOD walkthroughs. Automated legal puzzle moves and UI checks do not establish human difficulty or fun. Direct upload from the authoring room to the platform is not implemented; external ZIPs are extracted into the MOD folder.

## Further documentation

[Existing detailed development reference (Korean)](docs/DEVELOPMENT_REFERENCE.ko.md)

[Docs/FullPlayVerification.md](Docs/FullPlayVerification.md)

[Docs/HotelPrototype.md](Docs/HotelPrototype.md)

[Docs/ThemeMods.md](Docs/ThemeMods.md)

## Related repositories

[Puzzle-Spire](https://github.com/YSL-RyuDo/Puzzle-Spire), Public repository
