# Relay Yard — final HUD, hold-reset and audio settings

**Branch:** `cpsc386-project2-urp-test`  
**Stable checkpoint:** `cpsc386-project2-relay-yard` (do not overwrite).  
**Requires:** saved URP converted MainMenu/Game/Win scenes, Input System, Unity UI; Unity 6000.6.4f1.

## Safest procedure on the Windows Unity workstation

1. Stop Play, save, and close Unity.
2. From the Git checkout on `cpsc386-project2-urp-test`:

   ```bat
   cd /d "C:\Users\user\Documents\Project 2\RelayYard"
   git branch --show-current
   git status --short
   git pull --ff-only origin cpsc386-project2-urp-test
   ```

   If Git refuses the pull because of local changes, protect those changes before retrying. Do not use `git reset --hard` or `git clean`.

3. Open the URP project in Unity. Let scripts finish importing. **Resolve any red Console errors before running Editor tools.**
4. From Unity's Tools menu run **Relay Yard > Upgrade to Final HUD and Audio**. Confirm **Back Up and Upgrade**.
5. The tool copies `Assets/Scenes/{MainMenu,Game,Win}.unity` to uniquely named `Assets/InterfaceBackups/*-BeforeModernUI.unity` files, then adds a separate `Relay Yard Modern UI` Canvas to each scene. The old `Relay Yard Canvas` is retained **inactive** and can be restored manually if needed.
6. Open `Assets/Scenes/MainMenu.unity` and press Play. Test the menu, Audio, Game, pause, reset and Win paths below.
7. Save all scene changes; commit and push new .unity, .meta, texture and other assets **only to the URP test branch**. The GitHub script commit itself does not contain newly generated Canvas scene objects.

**Do not run** Assemble Lights Out Room, Upgrade UI In Existing Scenes, or Convert Saved Game Scene merely to install this final UI; those commands are unrelated and may replace or duplicate work.

## Final layout

- **MainMenu:** RELAY YARD; one rule: "Stepping on a plate also toggles its neighbors." START, AUDIO, QUIT.
- **Game upper band:** "Light all six indicators", circuits on/total, current elapsed time, saved best time.
- **Game lower band:** A/D Move, SPACE Jump, ESC Pause, clickable RESET control with R circular hold progress.
- **Pause:** Resume, Restart, Audio, Main Menu; Escape remains available.
- **Audio overlay:** Music and SFX sliders, independent mute/unmute, Reset Audio, Test SFX, Back; reachable from main menu and pause.
- **Win:** Clear Time, Best Time, NEW BEST when applicable, Play Again, Main Menu. No attempts counter or invented score.

## Reset contract

- Hold **R** continuously for **0.9 seconds** while the puzzle is unsolved and gameplay is running.
- The ring fills clockwise, starting at 12 o'clock. Releasing early cancels the actual pending reset *immediately*; the displayed ring drains to empty over **0.25 seconds**.
- Repeated taps never accumulate time toward a reset.
- Completing the hold reloads Game, restoring the initial board, player, gate and timer.
- Clicking the same reset control immediately reloads Game, without a hold.
- Hold R is disabled when the puzzle has been solved or when the game is paused. Pause's explicit Restart action still works.

## Exit availability

- The six-switch solve event immediately unlocks the exit and disables the blocking gate collider.
- The gate snaps fully clear of the doorway in the same frame as the solve event; the goal trigger accepts the player immediately, without waiting for a lift animation.
- This does **not** auto-complete the game on the sixth light by itself: the player must be overlapping the goal trigger. Both entering after solving and remaining inside it when solved are valid; `OnTriggerEnter2D` and `OnTriggerStay2D` share the completion check. The timer stops upon successful goal completion.

## Timing contract

- The elapsed clock starts when the playable Game scene becomes active, advances using `Time.deltaTime` and therefore freezes with `Time.timeScale=0`.
- It **does not stop** when the sixth light is lit: it stops on entering the goal beyond the raised gate.
- The best time changes **only on a successful goal entry**, if the completion was faster than the saved best.
- Restart creates a new elapsed timer at zero, without storing a completed result.
- The Win screen reads the last completed time from the current play session and the saved best time from PlayerPrefs.
- There is **no attempt counter**.

## Audio contract

- Music and SFX volume levels are independent.
- Each channel can be muted independently. Muting preserves its slider value, so unmuting restores that value.
- Reset Audio restores Music **55%**, SFX **75%**, and unmuted for both; it does not affect puzzle or best-time records.
- Levels/mutes persist across scenes and app restarts via PlayerPrefs.
- The original AudioDirector singleton and optional AudioMixer Music/SFX output groups are retained. `Assets/Resources/RelayMixer.mixer` must still be created/verified independently if it has not yet been saved locally.

## Local acceptance checklist

- [ ] Unity recompiles without red errors after pull.
- [ ] Three new `Relay Yard Modern UI` canvases exist; three prior canvases are inactive.
- [ ] Main menu displays only the single-rule sentence, plus Start/Audio/Quit.
- [ ] Main menu Audio overlay covers underlying controls; both sliders change the correct channels.
- [ ] Music mute/unmute preserves music volume; SFX mute/unmute preserves effects volume.
- [ ] Reset Audio returns to defaults; quit/relaunch preserves changes.
- [ ] Game top/bottom bands are readable at 1280x720 and a second 16:9 resolution; center gameplay remains visible.
- [ ] R held for less than 0.9s then released: no reset; circular ring drains; repeated taps don't accumulate.
- [ ] R held for 0.9s: exactly one reset, clock zero, initial lights restored and gate locked.
- [ ] Clicking RESET immediately performs the same restart.
- [ ] Escape pauses clock/movement/gate; opening Audio during pause and then leaving it works.
- [ ] Resuming from Audio or Escape does not leave an invisible audio modal active.
- [ ] R cannot reset during pause or after all six lights are on; pause-menu Restart remains available.
- [ ] As soon as all six lights turn on, the gate no longer blocks the player and the goal is eligible; the gate animation can still continue.
- [ ] After solving and crossing the goal, Win shows clear/best time; best survives app restart.
- [ ] Restarting or exiting to menu without a clear does **not** update best time.
- [ ] Gate URP light, particles, waveform, audio, scene navigation, and walls still work.
- [ ] Windows standalone build is launched and tested outside Unity.

## Source implementation

- `RelayRunClock.cs`: scene-local timer and persisted fastest goal clear.
- `ResetHoldController.cs`: new Gameplay/ResetHold Input Action, continuous hold and radial visual progress.
- `AudioDirector.cs`: two independent volume/mute states with reset and PlayerPrefs persistence.
- `AudioSettingsPanel.cs`: one reusable control panel implementation for Menu and Pause.
- `SceneUI.cs`: gameplay clock/circuits and Win time fields.
- `SceneLoader.cs`: records a completed run when the valid Win goal is entered.
- `RelayYardModernUIInstaller.cs`: Editor-only backup and in-place Canvas authoring.

**Verification status:** Source authored and GitHub-pushed. Unity Editor compilation, visual styling, UI wiring, and standalone behavior require the Windows Unity workstation. Never mark these tests passed merely because the source files exist.
