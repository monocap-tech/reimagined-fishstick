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
    // One-click, reproducible starting scenes. Run once; later changes should be made in the Editor.
    public static class RelayYardAssembler
    {
        private const string Root = "Assets/RelayYardGenerated";
        private const string SceneFolder = Root + "/Scenes";
        private const string ArtFolder = Root + "/Art";
        private const string DataFolder = Root + "/Data";
        private const string AnimationFolder = Root + "/Animations";
        private const string ControlsPath = "Assets/Input/RelayControls.inputactions";

        [MenuItem("Tools/Relay Yard/Assemble Playable Prototype")]
        public static void Assemble()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolders();
            AssetDatabase.Refresh();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (actions == null)
            {
                EditorUtility.DisplayDialog("Relay Yard", "Input Actions asset not imported. Install Input System package, let Unity compile, then retry.", "OK");
                return;
            }

            int groundLayer = EnsureGroundLayer();
            Sprite square = BuildSquareSprite();
            RelayDefinition blue = BuildRelayData("CyanRelay", "Cyan Power", new Color(0.12f, 0.89f, 0.85f));
            RelayDefinition gold = BuildRelayData("GoldRelay", "Gold Power", new Color(1f, 0.79f, 0.22f));
            RuntimeAnimatorController animations = BuildAnimator();

            CreateMenuScene();
            CreateGameplayScene(square, actions, animations, groundLayer, blue, gold);
            CreateWinScene();

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
                "Scenes, input references, Animator, two ScriptableObjects, physics, particle effects and procedural backdrop are configured. NEXT: Create Resources/RelayMixer.mixer with Music and SFX groups as documented, then test and export.",
                "OK");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Input");
            Directory.CreateDirectory("Assets/Editor");
            Directory.CreateDirectory("Assets/Scripts");
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
                throw new InvalidOperationException("ProjectSettings/TagManager.asset is required. Start from a standard Unity 6 2D project.");
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
            throw new InvalidOperationException("No free user layer for RelayGround.");
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

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
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

        private static RelayDefinition BuildRelayData(string filename, string title, Color color)
        {
            string path = DataFolder + "/" + filename + ".asset";
            RelayDefinition data = AssetDatabase.LoadAssetAtPath<RelayDefinition>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<RelayDefinition>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.EditorConfigure(title, color);
            EditorUtility.SetDirty(data);
            return data;
        }

        private static AnimationClip BuildClip(string name, float scaleY)
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
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"),
                AnimationCurve.Constant(0, 0.3f, scaleY));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorStateTransition Connect(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.025f;
            return transition;
        }

        private static RuntimeAnimatorController BuildAnimator()
        {
            AnimationClip idle = BuildClip("Idle", 1.0f);
            AnimationClip run = BuildClip("Run", 0.92f);
            AnimationClip jump = BuildClip("Jump", 1.15f);
            AnimationClip fall = BuildClip("Fall", 0.84f);

            string path = AnimationFolder + "/Player.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            var locomotion = machine.AddState("Locomotion");
            var blend = new BlendTree
            {
                name = "IdleRunBlend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            blend.AddChild(idle, 0f);
            blend.AddChild(run, 1f);
            AssetDatabase.AddObjectToAsset(blend, controller);
            locomotion.motion = blend;
            machine.defaultState = locomotion;

            AnimatorState jumpState = machine.AddState("Jump");
            jumpState.motion = jump;
            jumpState.AddStateMachineBehaviour<JumpFeedback>();
            AnimatorState fallState = machine.AddState("Fall");
            fallState.motion = fall;

            var locomotionJump = Connect(locomotion, jumpState);
            locomotionJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            locomotionJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalSpeed");

            var locomotionFall = Connect(locomotion, fallState);
            locomotionFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            locomotionFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");

            var jumpFall = Connect(jumpState, fallState);
            jumpFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");

            var fallGround = Connect(fallState, locomotion);
            fallGround.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void CreateCamera(Color color)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0, 0, -10);
            Camera cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = color;
            go.AddComponent<AudioListener>();
        }

        private static void AddAudioDirector()
        {
            new GameObject("Persistent Audio Director").AddComponent<AudioDirector>();
        }

        private static void CreateMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.07f, 0.1f, 0.2f));
            AddAudioDirector();
            var ui = new GameObject("Main Menu UI").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Menu);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/MainMenu.unity");
        }

        private static void CreateWinScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.06f, 0.17f, 0.14f));
            AddAudioDirector();
            var ui = new GameObject("Victory UI").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Win);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Win.unity");
        }

        private static SpriteRenderer MakeSprite(string label, Vector2 position, Vector2 scale,
            Color color, Sprite square)
        {
            var go = new GameObject(label);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            return renderer;
        }

        private static void SolidBlock(string label, Vector2 position, Vector2 size,
            Color color, Sprite square, int groundLayer)
        {
            SpriteRenderer art = MakeSprite(label, position, size, color, square);
            art.gameObject.layer = groundLayer;
            art.gameObject.AddComponent<BoxCollider2D>();
        }

        private static ParticleSystem MakeBurst(Transform parent, string label, Color tint)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.48f;
            main.startLifetime = 0.3f;
            main.startSpeed = 1.8f;
            main.startSize = 0.13f;
            main.startColor = tint;
            main.maxParticles = 40;
            var emission = system.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0.01f, 22) });
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.14f;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return system;
        }

        private static PlayerMotor CreatePlayer(Sprite square, InputActionAsset actions,
            RuntimeAnimatorController controller, int groundLayer)
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(-7.4f, -1.4f, 0);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.4f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.68f, 1f);

            var visual = MakeSprite("Visual", Vector2.zero, new Vector2(0.7f, 1f),
                new Color(0.45f, 0.77f, 1f), square);
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            Animator anim = visual.gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            var probe = new GameObject("Ground Probe").transform;
            probe.SetParent(player.transform, false);
            probe.localPosition = new Vector3(0, -0.56f, 0);

            ParticleSystem fx = MakeBurst(player.transform, "Jump Dust", new Color(0.72f, 0.93f, 1f));
            fx.transform.localPosition = new Vector3(0, -0.46f, 0);
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            motor.EditorConfigure(actions, probe, 1 << groundLayer, anim, fx);
            return motor;
        }

        private static RelayBeacon MakeRelay(string id, Vector2 position,
            RelayDefinition data, RelayController controller, Sprite square)
        {
            SpriteRenderer art = MakeSprite(id, position, new Vector2(0.53f, 0.53f),
                new Color(0.78f, 0.4f, 0.23f), square);
            BoxCollider2D hitbox = art.gameObject.AddComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            ParticleSystem fx = MakeBurst(art.transform, "Activation Sparks", data.ActiveColor);
            var beacon = art.gameObject.AddComponent<RelayBeacon>();
            beacon.EditorConfigure(id, data, controller, art, fx);
            return beacon;
        }

        private static void CreateGameplayScene(Sprite square, InputActionAsset actions,
            RuntimeAnimatorController animator, int groundLayer, RelayDefinition cyan, RelayDefinition gold)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.045f, 0.075f, 0.15f));
            AddAudioDirector();

            var backdrop = new GameObject("Procedural Starfield").AddComponent<SeededBackdrop>();
            backdrop.EditorConfigure(square, 386);

            SolidBlock("Floor", new Vector2(0, -2.45f), new Vector2(18, 0.9f),
                new Color(0.24f, 0.29f, 0.43f), square, groundLayer);
            SolidBlock("Jump Platform", new Vector2(-1.3f, -0.78f), new Vector2(2.3f, 0.3f),
                new Color(0.28f, 0.47f, 0.61f), square, groundLayer);
            SolidBlock("Small Obstacle", new Vector2(1.9f, -1.65f), new Vector2(0.75f, 0.7f),
                new Color(0.27f, 0.42f, 0.55f), square, groundLayer);

            CreatePlayer(square, actions, animator, groundLayer);

            RelayController controller = new GameObject("Puzzle Controller").AddComponent<RelayController>();
            SpriteRenderer exitRenderer = MakeSprite("Exit Gate", new Vector2(7.25f, -1.30f),
                new Vector2(0.7f, 1.4f), new Color(0.95f, 0.27f, 0.29f), square);
            BoxCollider2D gateTrigger = exitRenderer.gameObject.AddComponent<BoxCollider2D>();
            gateTrigger.isTrigger = true;
            ExitGate gate = exitRenderer.gameObject.AddComponent<ExitGate>();
            gate.EditorConfigure(exitRenderer);

            RelayBeacon a = MakeRelay("Relay A", new Vector2(-4.1f, -1.42f), cyan, controller, square);
            RelayBeacon b = MakeRelay("Relay B", new Vector2(-1.3f, 0.08f), gold, controller, square);
            RelayBeacon c = MakeRelay("Relay C", new Vector2(3.9f, -1.42f), cyan, controller, square);
            controller.EditorConfigure(new[] { a, b, c }, gate);

            var ui = new GameObject("Gameplay HUD").AddComponent<SceneUI>();
            ui.EditorConfigure(SceneUI.ScreenMode.Game);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Game.unity");
        }
    }
}
#endif
