#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RelayYard.EditorTools
{
    // This is a one-shot editor build tool, NOT the game's runtime procedural generator.
    // WARNING: regenerating overwrites the three generated scenes.
    public static class RelayYardAssembler
    {
        private const string Root = "Assets/RelayYardGenerated";
        private const string SceneFolder = Root + "/Scenes";
        private const string ArtFolder = Root + "/Art";
        private const string DataFolder = Root + "/Data";
        private const string AnimationFolder = Root + "/Animations";
        private const string ControlsPath = "Assets/Input/RelayControls.inputactions";

        [MenuItem("Tools/Relay Yard/Audit Generated Project")]
        public static void AuditGeneratedProject()
        {
            int errors = 0;
#if !ENABLE_INPUT_SYSTEM
            Debug.LogError("[Relay Yard Audit] Enable the Input System package in Player settings.");
            errors++;
#endif
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (actions == null || actions.FindActionMap("Gameplay", false) == null)
            {
                Debug.LogError("[Relay Yard Audit] Gameplay Input Actions not found.");
                errors++;
            }
            foreach (string name in new[] { "MainMenu", "Game", "Win" })
            {
                string path = SceneFolder + "/" + name + ".unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    Debug.LogError("[Relay Yard Audit] Missing scene " + path);
                    errors++;
                }
                bool enabled = false;
                foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
                    if (s.enabled && s.path == path) enabled = true;
                if (!enabled)
                {
                    Debug.LogError("[Relay Yard Audit] Scene not enabled for build: " + name);
                    errors++;
                }
            }
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimationFolder + "/Player.controller") == null)
            {
                Debug.LogError("[Relay Yard Audit] Missing Player.controller");
                errors++;
            }
            foreach (string name in new[] { "CyanSwitch", "GoldSwitch" })
            {
                if (AssetDatabase.LoadAssetAtPath<SwitchStyle>(DataFolder + "/" + name + ".asset") == null)
                {
                    Debug.LogError("[Relay Yard Audit] Missing SwitchStyle asset " + name);
                    errors++;
                }
            }
            var mixer = Resources.Load<UnityEngine.Audio.AudioMixer>("RelayMixer");
            if (mixer == null || mixer.FindMatchingGroups("Music").Length == 0 ||
                mixer.FindMatchingGroups("SFX").Length == 0)
            {
                Debug.LogError("[Relay Yard Audit] Create Resources/RelayMixer.mixer with Music and SFX groups.");
                errors++;
            }
            if (errors == 0)
                Debug.Log("[Relay Yard Audit] Asset/scene/mixer preflight passed. Play and standalone build still require testing.");
            else
                Debug.LogError("[Relay Yard Audit] " + errors + " setup issue(s) found.");
        }

        [MenuItem("Tools/Relay Yard/Assemble Lights Out Room")]
        public static void AssembleLightsOut() => Assemble();

        [MenuItem("Tools/Relay Yard/Assemble Playable Prototype")]
        public static void Assemble()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Regenerate Relay Yard",
                "This replaces the generated MainMenu, Game, and Win scenes with the six-plate Lights Out puzzle. " +
                "Any manual edits to those scenes will be overwritten. Continue?", "Regenerate", "Cancel"))
                return;

            EnsureFolders();
            AssetDatabase.Refresh();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (actions == null)
            {
                EditorUtility.DisplayDialog("Relay Yard",
                    "Gameplay Input Actions are not imported. Check the Input System package.", "OK");
                return;
            }
            int groundLayer = EnsureGroundLayer();
            Sprite square = BuildSquareSprite();
            SwitchStyle cyan = BuildStyle("CyanSwitch", "Cyan circuit",
                new Color(0.13f, 0.24f, 0.33f), new Color(0.38f, 0.92f, 0.90f));
            SwitchStyle gold = BuildStyle("GoldSwitch", "Gold circuit",
                new Color(0.29f, 0.24f, 0.17f), new Color(1.0f, 0.77f, 0.32f));
            RuntimeAnimatorController animator = BuildAnimator();
            if (square == null || animator == null)
                throw new InvalidOperationException("Square sprite or Animator creation failed.");

            CreateMenu();
            CreateGame(square, actions, animator, groundLayer, cyan, gold);
            CreateWin();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneFolder + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneFolder + "/Game.unity", true),
                new EditorBuildSettingsScene(SceneFolder + "/Win.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(SceneFolder + "/MainMenu.unity");
            EditorUtility.DisplayDialog("Relay Yard",
                "Six separate pressure plates, six indicator lights, Lights Out logic, a sliding gate, " +
                "a goal trigger, animation, particles, and a Perlin signal trace are generated. " +
                "Create Resources/RelayMixer.mixer with Music and SFX groups, audit, and then PLAYTEST.",
                "OK");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scripts");
            Directory.CreateDirectory("Assets/Input");
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(AnimationFolder);
        }

        private static int EnsureGroundLayer()
        {
            int existing = LayerMask.NameToLayer("RelayGround");
            if (existing >= 0) return existing;
            UnityEngine.Object[] settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (settings.Length == 0)
                throw new InvalidOperationException("Missing ProjectSettings/TagManager.asset");
            var serialized = new SerializedObject(settings[0]);
            SerializedProperty layers = serialized.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue)) continue;
                slot.stringValue = "RelayGround";
                serialized.ApplyModifiedProperties();
                return i;
            }
            throw new InvalidOperationException("No free user layer.");
        }

        private static Sprite BuildSquareSprite()
        {
            string path = ArtFolder + "/Square.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                var pixels = new Color[16 * 16];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null &&
                (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 16))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static SwitchStyle BuildStyle(string filename, string label, Color off, Color on)
        {
            string path = DataFolder + "/" + filename + ".asset";
            SwitchStyle style = AssetDatabase.LoadAssetAtPath<SwitchStyle>(path);
            if (style == null)
            {
                style = ScriptableObject.CreateInstance<SwitchStyle>();
                AssetDatabase.CreateAsset(style, path);
            }
            style.EditorConfigure(label, off, on);
            EditorUtility.SetDirty(style);
            return style;
        }

        private static AnimationClip BuildClip(string name, float[] times, float[] scaleX, float[] scaleY)
        {
            string path = AnimationFolder + "/" + name + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            clip.frameRate = 30;
            clip.wrapMode = WrapMode.Loop;
            var xs = new Keyframe[times.Length];
            var ys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
            {
                xs[i] = new Keyframe(times[i], scaleX[i]);
                ys[i] = new Keyframe(times[i], scaleY[i]);
            }
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"),
                new AnimationCurve(xs));
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"),
                new AnimationCurve(ys));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorStateTransition Connect(AnimatorState from, AnimatorState to)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = false;
            t.hasFixedDuration = true;
            t.duration = 0.06f;
            return t;
        }

        private static RuntimeAnimatorController BuildAnimator()
        {
            AnimationClip idle = BuildClip("Idle",
                new[] { 0f, 0.35f, 0.70f }, new[] { .70f, .71f, .70f },
                new[] { 1f, 1.035f, 1f });
            AnimationClip walk = BuildClip("Walk",
                new[] { 0f, .20f, .40f }, new[] { .70f, .73f, .70f },
                new[] { 1f, .92f, 1f });
            AnimationClip run = BuildClip("Run",
                new[] { 0f, .10f, .20f }, new[] { .72f, .80f, .72f },
                new[] { .94f, .86f, .94f });
            AnimationClip jump = BuildClip("Jump",
                new[] { 0f, .10f, .24f }, new[] { .76f, .65f, .70f },
                new[] { .90f, 1.13f, 1.07f });
            AnimationClip fall = BuildClip("Fall",
                new[] { 0f, .12f, .24f }, new[] { .70f, .74f, .70f },
                new[] { .95f, .91f, .95f });

            string path = AnimationFolder + "/Player.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ac.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            ac.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = ac.layers[0].stateMachine;
            AnimatorState locomotion = machine.AddState("Locomotion");
            var blend = new BlendTree
            {
                name = "Idle Walk Run Blend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            blend.AddChild(idle, 0f);
            blend.AddChild(walk, .5f);
            blend.AddChild(run, 1f);
            AssetDatabase.AddObjectToAsset(blend, ac);
            locomotion.motion = blend;
            machine.defaultState = locomotion;

            AnimatorState jumpState = machine.AddState("Jump");
            jumpState.motion = jump;
            jumpState.AddStateMachineBehaviour<JumpFeedback>();
            AnimatorState fallState = machine.AddState("Fall");
            fallState.motion = fall;
            var intoJump = Connect(locomotion, jumpState);
            intoJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            intoJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalSpeed");
            var intoFall = Connect(locomotion, fallState);
            intoFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            intoFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");
            var jumpFall = Connect(jumpState, fallState);
            jumpFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");
            var landed = Connect(fallState, locomotion);
            landed.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            EditorUtility.SetDirty(ac);
            return ac;
        }

        private static void CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(.55f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            go.AddComponent<AudioListener>();
        }

        private static void AddAudioDirector()
        {
            new GameObject("Persistent Audio Director").AddComponent<AudioDirector>();
        }

        private static void CreateMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(.06f, .10f, .18f));
            AddAudioDirector();
            var ui = new GameObject("Main Menu UI").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Menu);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/MainMenu.unity");
        }

        private static void CreateWin()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(.05f, .17f, .14f));
            AddAudioDirector();
            var ui = new GameObject("Victory UI").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Win);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Win.unity");
        }

        private static SpriteRenderer MakeSprite(string label, Vector2 position, Vector2 size,
            Color color, Sprite sprite)
        {
            var go = new GameObject(label);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            return renderer;
        }

        private static void CreateFloor(Sprite sprite, int layer)
        {
            var art = MakeSprite("Solid Floor", new Vector2(0f, -2.45f),
                new Vector2(18f, .9f), new Color(.23f, .29f, .41f), sprite);
            art.gameObject.layer = layer;
            art.gameObject.AddComponent<BoxCollider2D>();
        }

        private static PhysicsMaterial2D CreateSlick()
        {
            string path = ArtFolder + "/Slick.asset";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material == null)
            {
                material = new PhysicsMaterial2D("Slick");
                AssetDatabase.CreateAsset(material, path);
            }
            material.friction = 0f;
            material.bounciness = 0f;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static ParticleSystem MakeBurst(Transform parent, string label, Color tint)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = .55f;
            main.startLifetime = .45f;
            main.startSpeed = 2.6f;
            main.startSize = .13f;
            main.startColor = tint;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[0]);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = .14f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private static PlayerMotor CreatePlayer(Sprite sprite, InputActionAsset actions,
            RuntimeAnimatorController animator, int groundLayer)
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(-7.1f, -1.5f, 0f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var body = player.AddComponent<BoxCollider2D>();
            body.size = new Vector2(.68f, 1f);
            body.sharedMaterial = CreateSlick();

            var visual = MakeSprite("Visual", Vector2.zero, new Vector2(.7f, 1f),
                new Color(.52f, .84f, 1f), sprite);
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.sortingOrder = 7;
            var characterAnimator = visual.gameObject.AddComponent<Animator>();
            characterAnimator.runtimeAnimatorController = animator;

            var probe = new GameObject("Ground Probe").transform;
            probe.SetParent(player.transform, false);
            probe.localPosition = new Vector3(0f, -.54f, 0f);

            ParticleSystem dust = MakeBurst(player.transform, "Jump Dust", new Color(.70f, .93f, 1f));
            dust.transform.localPosition = new Vector3(0f, -.48f, 0f);
            var motor = player.AddComponent<PlayerMotor>();
            motor.EditorConfigure(actions, probe, 1 << groundLayer, characterAnimator, dust);
            return motor;
        }

        private static ExitGate CreateGate(Sprite sprite, Transform player)
        {
            var visual = MakeSprite("Exit Gate", new Vector2(7.2f, .45f),
                new Vector2(.75f, 4.9f), new Color(.42f, .25f, .18f), sprite);
            visual.sortingOrder = 4;
            var collider = visual.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            var glow = MakeSprite("Gate Proximity Halo", new Vector2(7.2f, .45f),
                new Vector2(1.4f, 5.3f), new Color(1f, .48f, .12f, .04f), sprite);
            glow.sortingOrder = 1;
            glow.transform.SetParent(visual.transform, true);
            var gate = visual.gameObject.AddComponent<ExitGate>();
            gate.EditorConfigure(visual, glow, player);
            var goal = MakeSprite("Goal Beyond Gate", new Vector2(8.25f, -1.5f),
                new Vector2(.6f, 1f), new Color(.25f, .88f, .79f, .35f), sprite);
            goal.sortingOrder = 2;
            var trigger = goal.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            goal.gameObject.AddComponent<ExitGoal>().EditorConfigure(gate);
            return gate;
        }

        private static PressurePlate CreatePlate(int i, float x, SwitchStyle style,
            LightsOutController puzzle, Sprite sprite, out SpriteRenderer lamp)
        {
            var plate = MakeSprite("Pressure Plate " + (i + 1),
                new Vector2(x, -1.96f), new Vector2(.56f, .14f),
                style.OffColor, sprite);
            plate.sortingOrder = 5;
            var trigger = plate.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 4f); // Thin sprite, taller foot-contact trigger
            ParticleSystem fx = MakeBurst(plate.transform, "Toggle Sparks", style.OnColor);
            var press = plate.gameObject.AddComponent<PressurePlate>();
            press.EditorConfigure(i, puzzle, plate, style, fx);

            var halo = MakeSprite("Indicator Halo " + (i + 1),
                new Vector2(x, 1.42f), new Vector2(.74f, .74f),
                new Color(.36f, .67f, .81f, .09f), sprite);
            halo.sortingOrder = 1;
            lamp = MakeSprite("Indicator Light " + (i + 1),
                new Vector2(x, 1.42f), new Vector2(.48f, .48f),
                style.OffColor, sprite);
            lamp.sortingOrder = 6;
            return press;
        }

        private static void CreateGame(Sprite sprite, InputActionAsset actions,
            RuntimeAnimatorController animator, int groundLayer, SwitchStyle cyan, SwitchStyle gold)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(.045f, .073f, .13f));
            AddAudioDirector();
            CreateFloor(sprite, groundLayer);
            PlayerMotor player = CreatePlayer(sprite, actions, animator, groundLayer);

            var signal = new GameObject("Seeded Perlin Signal").AddComponent<SeededSignal>();
            signal.transform.position = new Vector3(0f, 2.65f, 0f);
            signal.EditorConfigure(386);

            var gate = CreateGate(sprite, player.transform);
            var puzzle = new GameObject("Lights Out Controller").AddComponent<LightsOutController>();
            var plates = new PressurePlate[6];
            var lamps = new SpriteRenderer[6];
            for (int i = 0; i < 6; i++)
            {
                float x = -5.4f + i * 2.1f;
                plates[i] = CreatePlate(i, x, i % 2 == 0 ? cyan : gold,
                    puzzle, sprite, out lamps[i]);
            }
            var winFxRoot = new GameObject("Victory Sparks");
            winFxRoot.transform.position = new Vector3(0f, 1.45f, 0f);
            ParticleSystem winFx = MakeBurst(winFxRoot.transform, "Completion Burst",
                new Color(.75f, 1f, .63f));
            puzzle.EditorConfigure(plates, lamps, gate, winFx, signal);

            var ui = new GameObject("Gameplay HUD").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Game);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Game.unity");
        }
    }
}
#endif
