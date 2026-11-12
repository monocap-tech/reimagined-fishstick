#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace RelayYard.EditorTools
{
    /// <summary>
    /// One-shot, idempotent conversion of the SAVED Assets/Scenes/Game.unity.
    /// The conversion NEVER reruns the puzzle assembler, replaces the UI,
    /// or modifies the original Project 2 branch.
    /// Requires Universal RP package 17.6 on Unity 6000.6.
    /// </summary>
    public static class RelayYardURP2DMigrator
    {
        private const string GamePath = "Assets/Scenes/Game.unity";
        private const string BackupFolder = "Assets/URPBackups";
        private const string BackupPath = BackupFolder + "/Game-BeforeURP2D.unity";
        private const string SettingsFolder = "Assets/Settings";
        private const string MaterialsFolder = "Assets/Materials";
        private const string PipelinePath = SettingsFolder + "/RelayYardURP.asset";
        private const string RendererPath = SettingsFolder + "/RelayYard2DRenderer.asset";
        private const string LitMaterialPath = MaterialsFolder + "/RelaySpriteLit.mat";
        private const string UnlitMaterialPath = MaterialsFolder + "/RelaySpriteUnlit.mat";
        private const string ParticlesMaterialPath = MaterialsFolder + "/RelayParticlesUnlit.mat";

        [MenuItem("Tools/Relay Yard/URP Test/Convert Saved Game Scene")]
        public static void ConvertSavedGame()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play before migrating.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GamePath) == null)
            {
                Debug.LogError("[Relay URP] Missing " + GamePath +
                               ". Save the playable game first.");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Relay Yard: URP 2D experiment",
                    "This edits ONLY the experimental project's saved Game scene and " +
                    "render pipeline settings. A Game scene backup is saved before edits. " +
                    "It does NOT regenerate your switches, character, UI or other scenes. " +
                    "Continue?",
                    "Back Up and Convert", "Cancel"))
                return;

            if (!AssetDatabase.IsValidFolder(BackupFolder))
                AssetDatabase.CreateFolder("Assets", "URPBackups");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BackupPath) == null)
            {
                if (!AssetDatabase.CopyAsset(GamePath, BackupPath))
                {
                    Debug.LogError("[Relay URP] Could not back up Game scene. No conversion performed.");
                    return;
                }
                AssetDatabase.SaveAssets();
            }

            string previousScene = SceneManager.GetActiveScene().path;
            try
            {
                // Fail before opening/changing the scene if the 2D shaders are absent.
                Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (litShader == null || unlitShader == null)
                    throw new InvalidOperationException(
                        "URP 2D Sprite Lit/Unlit shaders are missing. " +
                        "Confirm the Universal RP package was imported.");

                EnsureFolder(SettingsFolder, "Settings");
                EnsureFolder(MaterialsFolder, "Materials");

                var pipeline = EnsurePipeline();
                var lit = EnsureMaterial(LitMaterialPath, litShader);
                var unlit = EnsureMaterial(UnlitMaterialPath, unlitShader);
                var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                var particles = particleShader != null
                    ? EnsureMaterial(ParticlesMaterialPath, particleShader)
                    : unlit;

                // With all required assets created, activate the 2D Renderer.
                GraphicsSettings.defaultRenderPipeline = pipeline;
                // The current quality level must not override our 2D renderer.
                QualitySettings.renderPipeline = pipeline;

                var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
                Transform player = FindInScene(scene, "Player");
                Transform gateTransform = FindInScene(scene, "Exit Gate");
                Transform floor = FindInScene(scene, "Solid Floor");
                if (player == null || gateTransform == null || floor == null)
                    throw new InvalidOperationException(
                        "Required Player, Exit Gate or Solid Floor is missing. " +
                        "The original scene backup is still intact.");

                // Convert ONLY environmental sprites to Lit; controls remain Unlit.
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var sprite in root.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        bool receiveLight = sprite.name == "Exit Gate" ||
                                            sprite.name == "Solid Floor" ||
                                            sprite.name.StartsWith("Wall Panel ") ||
                                            sprite.name.StartsWith("Boundary Wall ");
                        sprite.sharedMaterial = receiveLight ? lit : unlit;
                        EditorUtility.SetDirty(sprite);
                    }

                    foreach (var particle in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
                    {
                        particle.sharedMaterial = particles;
                        EditorUtility.SetDirty(particle);
                    }
                }

                // Remove the fake emitter visually, preserving its serialized reference.
                var halo = FindInScene(scene, "Gate Proximity Halo");
                if (halo != null) halo.gameObject.SetActive(false);
                var gate = gateTransform.GetComponent<ExitGate>();
                if (gate == null)
                    throw new InvalidOperationException("Exit Gate has no ExitGate component.");
                gate.EditorEnableURP2DLighting();
                EditorUtility.SetDirty(gate);

                // A 2D Light physically follows the player's Transform (rather than
                // changing the gate's whole-color value by a distance calculation).
                var lantern = EnsureChild(player, "Lantern Light 2D");
                lantern.localPosition = Vector3.zero;
                var lanternLight = GetOrAddLight(lantern.gameObject);
                lanternLight.lightType = Light2D.LightType.Point; // UI calls this Spot
                lanternLight.color = new Color(1f, 0.718f, 0.302f);
                lanternLight.intensity = 1.35f;
                lanternLight.pointLightInnerRadius = 0.65f;
                lanternLight.pointLightOuterRadius = 5.8f;
                lanternLight.pointLightInnerAngle = 360f;
                lanternLight.pointLightOuterAngle = 360f;
                EditorUtility.SetDirty(lanternLight);

                var ambient = FindInScene(scene, "Room Ambient Light 2D");
                if (ambient == null)
                    ambient = new GameObject("Room Ambient Light 2D").transform;
                var ambientLight = GetOrAddLight(ambient.gameObject);
                ambientLight.lightType = Light2D.LightType.Global;
                ambientLight.intensity = 0.38f;
                ambientLight.color = new Color(0.50f, 0.66f, 0.82f);
                EditorUtility.SetDirty(ambientLight);

                Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
                if (square == null)
                    throw new InvalidOperationException("Assets/Sprites/Square.png is missing.");
                // Physical side boundaries stay outside the plate/goal traversal.
                MakePanel(scene, "Boundary Wall Left", square, lit,
                    new Vector2(-8.86f, 0.25f), new Vector2(0.38f, 5.05f),
                    new Color(0.30f, 0.39f, 0.48f), 2, true);
                MakePanel(scene, "Boundary Wall Right", square, lit,
                    new Vector2(8.86f, 0.25f), new Vector2(0.38f, 5.05f),
                    new Color(0.30f, 0.39f, 0.48f), 2, true);

                for (int i = 0; i < 7; i++)
                {
                    MakePanel(scene, "Wall Panel " + (i + 1), square, lit,
                        new Vector2(-7.5f + i * 2.5f, 0.25f),
                        new Vector2(2.38f, 5.0f),
                        i % 2 == 0
                            ? new Color(0.16f, 0.23f, 0.31f)
                            : new Color(0.12f, 0.19f, 0.27f),
                        -12, false);
                }

                var gateSprite = gateTransform.GetComponent<SpriteRenderer>();
                if (gateSprite != null)
                {
                    gateSprite.sharedMaterial = lit;
                    gateSprite.color = new Color(0.57f, 0.46f, 0.36f);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();

                Debug.Log("[Relay URP] Converted Game scene. Native 2D Renderer active, " +
                          "player lantern/global light/walls/materials installed. " +
                          "Original game saved at " + BackupPath +
                          ". Check Console, light falloff, gate movement, UI, SFX and build.");
                EditorUtility.DisplayDialog("URP 2D scene updated",
                    "Open Assets/Scenes/Game.unity in Play mode. Walk toward the gate: " +
                    "the warm light should move WITH the player and illuminate the gate/walls. " +
                    "Your Game scene backup is in Assets/URPBackups. " +
                    "Then check pause, win and standalone build.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Relay URP] Conversion did not complete: " + ex +
                               ". Original scene backup: " + BackupPath);
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousScene) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScene) != null)
                    EditorSceneManager.OpenScene(previousScene);
            }
        }

        [MenuItem("Tools/Relay Yard/URP Test/Audit Lighting Setup")]
        private static void AuditLighting()
        {
            var asset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererPath);
            var game = AssetDatabase.LoadAssetAtPath<SceneAsset>(GamePath);
            bool good = asset != null && renderer != null && game != null;
            Debug.Log(good
                ? "[Relay URP Audit] URP Asset, Renderer2DData and Game scene exist. " +
                  "Check Play mode for light falloff and confirm all quality overrides."
                : "[Relay URP Audit] Incomplete setup. Run Convert Saved Game Scene " +
                  "or inspect Graphics/Quality and Assets/Settings.");
        }

        private static void EnsureFolder(string path, string name)
        {
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets", name);
        }

        private static UniversalRenderPipelineAsset EnsurePipeline()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                if (pipeline == null)
                    throw new InvalidOperationException("URP failed to create a pipeline.");
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            else
            {
                // Re-running the converter should never silently change renderers
                // that an artist deliberately adjusted in the existing URP Asset.
                var serialized = new SerializedObject(pipeline);
                var list = serialized.FindProperty("m_RendererDataList");
                if (list == null || list.arraySize == 0 ||
                    list.GetArrayElementAtIndex(0).objectReferenceValue != renderer)
                    throw new InvalidOperationException(
                        "Existing RelayYardURP asset is not configured with RelayYard2DRenderer. " +
                        "Inspect Assets/Settings manually instead of replacing it.");
            }

            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(pipeline);
            return pipeline;
        }

        private static Material EnsureMaterial(string path, Shader shader)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
                material.shader = shader;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (item.name == name)
                        return item;
            return null;
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, false);
            }
            return child;
        }

        private static Light2D GetOrAddLight(GameObject obj)
        {
            Light2D light = obj.GetComponent<Light2D>();
            return light != null ? light : obj.AddComponent<Light2D>();
        }

        private static void MakePanel(Scene scene, string name, Sprite sprite, Material material,
            Vector2 center, Vector2 dimensions, Color tint, int sortingOrder, bool solid)
        {
            var found = FindInScene(scene, name);
            if (found == null)
                found = new GameObject(name).transform;
            found.position = new Vector3(center.x, center.y, 0f);
            found.localScale = new Vector3(dimensions.x, dimensions.y, 1f);

            var sr = found.GetComponent<SpriteRenderer>();
            if (sr == null) sr = found.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = material;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            if (solid)
            {
                var collision = found.GetComponent<BoxCollider2D>();
                if (collision == null) collision = found.gameObject.AddComponent<BoxCollider2D>();
                collision.isTrigger = false;
            }
        }
    }
}
#endif
