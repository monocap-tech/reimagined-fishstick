# Relay Yard — Project 2 submission checklist

Submit **three distinct deliverables** as required by the course instructions:

1. **Playable build:** ZIP the complete tested Windows standalone build directory (the executable **and** its data/runtime files), or an accepted WebGL build if chosen.
2. **Source:** GitHub release branch URL with the exact tested commit SHA, or a source ZIP containing `Assets/`, `Packages/`, `ProjectSettings/`, and all required Unity `.meta` files. Do not include `Library/`, `Temp/`, `obj/`, `.vs/`, or disposable build caches.
3. **Documentation:** individually reviewed DOCX or PDF technical report covering mechanics/design, scripts, scenes, rubric mapping, testing, asset sources, and AI assistance.

Proposed hand-in folder (local only; not committed to source Git):

```text
Project2-Submission/
  RelayYard-Windows.zip
  RelayYard-Documentation.pdf
  SOURCE.txt
```

## Release preparation

- [x] Preserve the URP development checkpoint on `cpsc386-project2-urp-test`.
- [ ] Open **`cpsc386-project2-release`** in Unity and allow a clean reimport.
- [ ] Verify the current three scenes, their scripts, animations and shader assets are intact after removing generator scripts/unused prototype assets.
- [x] Commit `Assets/Resources/RelayMixer.mixer`, its `.meta`, and Music/SFX groups. **Runtime assignment of the AudioSources to those groups still needs explicit verification.**
- [x] Commit `Assets/Resources/PuzzleClear.wav` and its `.meta`. **Still verify the cue in the final player and resolve its copyright/distribution status.**
- [ ] Run full gameplay and audio regression tests, including hold R, pause, gate animation, goal trigger overlap and best-time persistence.
- [ ] Build from the **release branch** with Build Profiles (MainMenu, Game, Win). Unzip to a separate folder and launch the player **outside Unity**.
- [ ] Verify no missing references/Console errors, no development Editor utilities, and a clean source tree.
- [ ] Finish the DOCX/PDF technical report, attach valid screenshots/testing record, cite `THIRD_PARTY_NOTICES.md`, and **disclose ChatGPT assistance** in the manner required by the syllabus.
- [ ] Prepare source URL and tested commit ID, then submit all three deliverables and any required peer-review copy.

**Original due date recorded from course materials:** Friday, November 13, 2026, at 11:59 PM PST (now past). Confirm the submission portal's current policy before uploading.
