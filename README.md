# Moonlit Hotel

[English](README.md), [Korean](README.ko.md), [Japanese](README.ja.md)

A mystery adventure combining investigation, dialogue and puzzles with local story authoring.

| Project | Details |
|---|---|
| Status | Playable prototype |
| Stack | C#, Unity, JSON / PNG |

[Explore the case](https://ysl-ryudo.github.io/en/work/moonlit/), [Portfolio](https://ysl-ryudo.github.io/en/)

## My contribution

Implemented investigation/dialogue/puzzle progression and separated puzzle domain rules from presentation.

Built puzzle clear-validation and story editing/export, with independent MOD save state.

Implemented local JSON/PNG content loading and theme switching. The separate platform backend is not part of this client's contribution claim.

## Contribution evidence

[Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

Commits are evidence of the stated changes, not a percentage-of-authorship score. Third-party assets and teammates' work remain separately credited.

## Implementation and design

A mystery adventure combines investigation, dialogue and puzzles. Authoring and loading new stories should not mix their save state with the base game.

I separated puzzle domain rules from presentation, implemented clear-validation and story editing/export, and gave local MOD stories independent save state.

| Area | Implementation |
|---|---|
| Rules | Puzzle domain rules and clear validation |
| Presentation | Investigation, dialogue and puzzle UI |
| Story content | JSON/PNG loading, editor/export and independent local-story saves |

## How the code works and why it is structured this way

These notes connect the inspected implementation to the constraints it addresses and its tradeoffs. Explanations of design consequences are code analysis, not a claim that every alternative was prototyped. Contribution links identify my changes within team projects; source links retain their original access permissions.

### Puzzle rules do not depend on the animation finishing

**Implementation**

PuzzleTurnResolver works on the board and composes connected-block finding, removal, gravity, spawning and special-block rules through explicit dependencies. It checks whether the state machine accepts input before resolving a selection and returns a turn result.

**Design rationale**

A rule implemented only inside an animation callback is difficult to inspect without running the scene. A C# domain layer makes board transitions testable without scene presentation, and separates the decision about what happened from how it is shown. The input-state check gives turn entry a single explicit rule.

**Alternatives and the current tradeoff**

Putting rules in MonoBehaviour callbacks minimizes boundaries in a small prototype. A separate domain layer needs more plumbing but allows rule changes and tests without reproducing every scene condition. The current resolver uses that separation; presentation still has to consume its results correctly.

**Tradeoffs and verification**

The cost is more interfaces, result data and synchronization between domain state and presentation. A passing rule test does not establish correct animation timing or input behavior in the complete scene; both layers need their own checks.

[Puzzle-Spire/PuzzleTurnResolver.cs](https://github.com/YSL-RyuDo/Puzzle-Spire/blob/6374c613fbce11db7b1b0927098d372b480bd420/Assets/Game/Domain/Puzzle/Rules/PuzzleTurnResolver.cs)

### Changing appearance should not create a new campaign

**Implementation**

Theme MODs replace art while sharing the base save. Content MODs use their own ID/version save boundary. The theme loader rejects content-file declarations and validates asset paths, regions and image limits; missing replacements fall back to base assets.

**Design rationale**

A cosmetic change should not discard progress, whereas altered content may make that progress incompatible. Distinguishing those cases in the format keeps save semantics explicit for both the editor and runtime instead of inferring them from whichever files happen to be present.

**Alternatives and the current tradeoff**

Isolating every MOD's save avoids sharing but needlessly separates progress for cosmetic changes. Sharing all saves risks incompatible content interpreting the same progress differently. Splitting theme and content formats makes that distinction explicit, with versioning work concentrated on content changes.

**Tradeoffs and verification**

The current design loads one MOD at a time, which avoids defining precedence across combinations but limits composition. Path and image checks constrain inputs; they do not establish support for arbitrary scripts or a published online workshop.

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

[Puzzle-Spire](https://github.com/YSL-RyuDo/Puzzle-Spire), Private, access required
