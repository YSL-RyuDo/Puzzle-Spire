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
