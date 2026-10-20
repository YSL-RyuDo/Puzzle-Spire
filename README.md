# Moonlit Hotel / 잔월 호텔

[English](README.md) · [Korean](README.ko.md) · [Japanese](README.ja.md)

A playable Unity mystery-adventure prototype built on Puzzle Spire's block-puzzle rules, with in-game story and puzzle authoring.

![Moonlit Hotel / 잔월 호텔](Docs/Images/base-dialogue.png)

## My contribution

- Implemented investigation/dialogue/puzzle progression and separated puzzle domain rules from presentation.
- Built puzzle clear-validation and story editing/export, with independent MOD save state.
- Implemented local JSON/PNG content loading and theme switching. The separate platform backend is not part of this client's contribution claim.

## Contribution evidence

- [Documented source baseline](https://github.com/YSL-RyuDo/Puzzle-Spire/tree/70129f77f42290735ff768192b63a08558fb2d84)

Commits are evidence of the stated changes, not a percentage-of-authorship score. Third-party assets and teammates' work remain separately credited.

## Open and inspect

Open the project with the Unity version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). Keep its package lock and serialized assets intact. Select the documented branch before opening Unity.

- Unity scene: [Assets/Game/Scenes/Hotel.unity](Assets/Game/Scenes/Hotel.unity)

## Development and verification scope

Playable prototype. The README's 2026-10-08 record reports 141 EditMode tests; subsequent documentation records story-MOD walkthroughs. Automated legal puzzle moves and UI checks do not establish human difficulty or fun. Direct upload from the authoring room to the platform is not implemented; external ZIPs are extracted into the MOD folder.

## Further documentation

- [Existing detailed development reference (Korean)](docs/DEVELOPMENT_REFERENCE.ko.md)
- [Docs/FullPlayVerification.md](Docs/FullPlayVerification.md)
- [Docs/HotelPrototype.md](Docs/HotelPrototype.md)
- [Docs/ThemeMods.md](Docs/ThemeMods.md)
