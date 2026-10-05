# Unity assembly

## Recommended route: generate once

1. Open the existing project in Unity 6 LTS. Back up any files with matching names before copying.
2. Download the repository ZIP, extract it, and merge its `Assets` contents into your project's `Assets` directory. Copy the Markdown files, `.gitignore`, and `.gitattributes` to the project root. Do not replace the existing package manifest or ProjectSettings.
3. Open **Window → Package Manager**. Install/confirm Input System, Unity UI (uGUI), and 2D Tilemap Editor. Use the versions offered for your editor. Canvas UI here uses `UnityEngine.UI.Text` and Unity's built-in font; TextMeshPro essentials are not required.
4. In **Edit → Project Settings → Player → Other Settings**, choose **Input System Package (New)** for Active Input Handling; restart if prompted. Resolve all Console compile errors before assembly.
5. Choose **Tools → Neon Range → Assemble Prototype**. Save your current scene if Unity requests it. The helper will create `Assets/NeonRangeGenerated` and open its MainMenu scene.
6. Open **Edit → Project Settings → Physics 2D → Layer Collision Matrix**. Ensure `Player–ArenaWall`, `PlayerBullet–ArenaWall`, and `PlayerBullet–Enemy` are enabled. Recommended: disable all other pairs among these four layers. These settings are persistent project settings and should be committed.
7. Select a **16:9** Game view resolution such as 1280×720. Press Play. Click Start and test using TESTING.md.
8. Open **File → Build Profiles**. Confirm MainMenu is enabled at index 0 and Gameplay is enabled next. If your active profile overrides the shared Scene List, update that profile too. Disable unrelated scenes for the submission build.

Generated assets are ordinary editable Unity assets. The helper is Editor-only and is not included in the player build. It never replaces an existing generated folder. To rebuild deliberately, first back up any changes to generated assets, then delete **only** `Assets/NeonRangeGenerated` inside Unity and run the helper again. If assembly failed partway, resolve the Console error before rebuilding.

## Generated scene contents

| Scene | Object | Setup |
|---|---|---|
| MainMenu | Main Camera | Orthographic size 6, position (0,0,-10), dark clear color |
| MainMenu | Canvas | Overlay UI, title, saved best score, Start and Quit Game buttons |
| MainMenu | MenuManager | MainMenu, MenuBestScore; buttons have saved persistent callbacks |
| Both | EventSystem | InputSystemUIInputModule with its default UI actions; no StandaloneInputModule |
| Gameplay | ArenaGrid / Walls | Grid, Tilemap, TilemapRenderer, TilemapCollider2D; border cells x=-8..7, y=-5..4 |
| Gameplay | Player / Muzzle | Cyan square at (0,-2.8); Dynamic Rigidbody2D, gravity 0, freeze rotation, BoxCollider2D |
| Gameplay | BulletPool | Prewarms 12 Bullet prefab instances at runtime |
| Gameplay | Target 1–3 | Pink trigger boxes tagged Enemy, each with EnemyTarget and pulse material |
| Gameplay | GameManager | ScoreManager and PauseManager |
| Gameplay | Canvas | Score, best score, controls; hidden PausePanel with Resume and Quit to Menu |

Three target respawn rectangles are (-5.5,0.8)–(-2.5,2.8), (-1.5,0.8)–(1.5,2.8), and (2.5,0.8)–(5.5,2.8). These stay inside the walls and separate the lanes.

## Inspector wiring reference / manual fallback

If you prefer manual assembly, use the supplied scripts and Input Actions without the helper. Create the same components as above and assign the following references. No generated C# input wrapper and no PlayerInput component are needed: the scripts enable and read InputActionReference objects directly.

| Component | Serialized field | Assign |
|---|---|---|
| PlayerController | moveAction | PlayerControls → Gameplay/Move |
| PlayerController | fireAction | PlayerControls → Gameplay/Fire |
| PlayerController | bulletPool | BulletPool scene component |
| PlayerController | muzzle | Child transform above the player's collider |
| BulletPool | bulletPrefab | Bullet prefab's Bullet component |
| EnemyTarget | scoreManager | GameManager's ScoreManager |
| EnemyTarget | minimum / maximum | Per-target rectangle listed above |
| ScoreManager | scoreText / bestScoreText | Canvas Text components |
| PauseManager | pauseAction | PlayerControls → Gameplay/Pause |
| PauseManager | pauseMenuUI | Hidden PausePanel GameObject |
| MenuBestScore | bestScoreText | MainMenu's best-score Text component |

Create action references from the input asset's imported action subassets (expand its Project window arrow), or use the generated reference assets after running the helper. Do not enable **Generate C# Class**; the scripts do not require it.

Create custom tags `Enemy` and `PlayerBullet`, and layers `Player`, `PlayerBullet`, `Enemy`, and `ArenaWall`. Player uses built-in tag `Player`. Enemy targets use tag/layer `Enemy`. Bullet uses tag/layer `PlayerBullet`; walls use layer `ArenaWall`.

For the Bullet prefab: SpriteRenderer, Dynamic Rigidbody2D (gravity 0, freeze rotation, continuous collision detection), BoxCollider2D (**Is Trigger**), and Bullet. Size approximately 0.16×0.38 world units. Keep the pool object's transform at origin with unit scale. The script positions and activates each shot, then deactivates it on a hit, wall contact, or two-second timeout. It never destroys a bullet.

For the tilemap: create a full-square sprite with 32 pixels per unit, a Tile with Collider Type **Grid**, then paint all four boundary walls on one Tilemap. Add TilemapCollider2D and leave **Is Trigger off**. Keep Grid and Tilemap transforms at origin with unit scale. Set camera size 6. Use an unlit sprite material on ordinary sprites and the tilemap so they are visible without requiring a configured 2D lighting renderer.

For the Week 6 effect: create a Material using **NeonRange/NeonPulse** and assign it to the target SpriteRenderers. Targets visibly pulse in brightness during play. This is a custom material/shader effect, not a bloom effect. If your instructor prefers a literal 2D Light, use a URP 2D Renderer, add a Global Light 2D, and assign Sprite-Lit-Default to the affected sprites; verify visibility before replacing the custom effect.

Wire button On Click callbacks: Start → MainMenu.StartGame; Quit Game → MainMenu.QuitGame; Resume → PauseManager.Resume; Quit to Menu → PauseManager.QuitToMenu. Keep PauseManager on the always-active GameManager, not on the hidden panel.

## Common import issues

- Missing `UnityEngine.InputSystem`: install Input System and allow Unity to recompile.
- Missing `UnityEngine.UI`: install Unity UI (uGUI).
- Missing Tilemap types: confirm the 2D Tilemap package and editor modules are installed.
- Buttons do not respond: confirm EventSystem has InputSystemUIInputModule with assigned default UI actions, and remove StandaloneInputModule.
- No Tools command: resolve the first Console compile error; keep the helper under an `Editor` folder.
- Black ordinary sprites: check their unlit material. The helper assigns unlit materials to avoid relying on an unconfigured Global Light.
- Solid magenta targets: shader import failed; inspect the Console shader errors before building.
- Player passes through walls or bullets do not score: inspect layer matrix, tags, Rigidbody2D, and trigger settings above.
- Scene-loading error: add both scenes to the active Build Profile Scene List with the exact names MainMenu and Gameplay.
