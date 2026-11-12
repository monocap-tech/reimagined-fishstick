# Project 2 submission layout

The course requires THREE deliverables, not a single copy of the entire Unity editor project:

1. **Playable Windows or WebGL build** packaged and tested outside the Unity Editor.
2. **Source** as a working GitHub branch or separate source ZIP.
3. **DOCX or PDF documentation**, describing scripts, scenes, objects, rubric mapping, external assets and assistance.

A clean local hand-in folder can look like:

```text
Project2-Submission/
  RelayYard-Windows.zip          <-- zip the entire tested Windows build folder
  RelayYard-Documentation.pdf    <-- final, personally reviewed report
  SOURCE.txt                     <-- URL of source branch and tested commit
```

Do **not** submit `Library/`, `Temp/`, `obj/`, or a bare standalone .exe without its accompanying data folder. Keep packaged builds out of source Git; `Build/` is ignored by .gitignore. If Canvas permits a source link, the GitHub branch is sufficient *after* the Unity-generated .unity/.meta/.anim/.asset and Resources/RelayMixer.mixer files have been committed.

Keep the Unity asset structure aligned to the course midterm example:

```text
RelayYard/
  Assets/
    Scenes/           MainMenu.unity, Game.unity, Win.unity
    Scripts/          PlayerMotor, SceneLoader, LightsOutBoard, ...
    Input/            RelayControls.inputactions
    Animations/       Player.controller and clips
    Sprites/          Square.png
    Materials/        Slick.asset (2D physics material)
    Data/             CyanSwitch.asset, GoldSwitch.asset
    Audio/            reserved for imported audio (original audio is code-generated)
    Prefabs/          reserved for reusable scene objects
    Tiles/            reserved for tiles and tilemaps
    Resources/        RelayMixer.mixer (create manually)
    Editor/           assembler, audit, and logic self-test tools
  Packages/
  ProjectSettings/
```

Note that a previously generated folder `Assets/RelayYardGenerated/` may remain from an earlier prototype on your workstation. The revised assembler writes to standard `Assets/Scenes`, `Assets/Sprites`, `Assets/Animations`, `Assets/Data` and `Assets/Materials`. Do not delete the old folder until you have confirmed that the new scenes are present, build correctly, and no active references depend on the old assets. Do not use `git clean` to handle generated Unity files.

The source contains an independent `SceneLoader.cs`, analogous to the *Lantern Dash* study guide arrangement: the Menu/Game/Win UI components invoke its named scene load methods; the goal has a trigger-configured instance that loads Win only after the solid gate has lifted. `ExitGoal.cs` remains as a legacy script for now and is not attached by the updated assembler.

Update `TESTING.md` after actual editor and standalone tests; the presence of the files and folder tree alone does not constitute a passing build. AI assistance must be disclosed in the final individually reviewed documentation, as required by the course syllabus.
