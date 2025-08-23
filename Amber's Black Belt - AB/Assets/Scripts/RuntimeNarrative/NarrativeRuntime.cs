using System;
using System.Collections.Generic;
using UnityEngine;
using Narrative;
#if NARRATIVE_USE_YAMLDOTNET
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
#endif

namespace NarrativeRuntime
{
    public interface IGameScene
    {
        string Id { get; }
        Sprite Background { get; }
    }

    public class RuntimeStoryScene : IGameScene
    {
        public string Id { get; internal set; }
        public Sprite Background { get; internal set; }
        public List<Narrative.SentenceData> Sentences { get; internal set; }
        public string NextId { get; internal set; }
    }

    public class RuntimeChooseScene : IGameScene
    {
        public string Id { get; internal set; }
        public Sprite Background { get; internal set; }
        public List<Narrative.ChoiceData> Choices { get; internal set; }
    }

    public class SpeakerRuntime
    {
        public string Id;
        public string Name;
        public Color Color;
        public List<Sprite> Sprites = new();
        public SpriteController Prefab;
    }

    public static class NarrativeRegistry
    {
        public static readonly Dictionary<string, RuntimeStoryScene> StoryScenes = new();
        public static readonly Dictionary<string, RuntimeChooseScene> ChooseScenes = new();
        public static readonly Dictionary<string, Sprite> Backgrounds = new();
        public static readonly Dictionary<string, SpeakerRuntime> Speakers = new();
        public static string StartSceneId;

        public static bool TryGet(string id, out IGameScene scene)
        {
            if (string.IsNullOrEmpty(id)) { scene = null; return false; }
            if (StoryScenes.TryGetValue(id, out var s)) { scene = s; return true; }
            if (ChooseScenes.TryGetValue(id, out var c)) { scene = c; return true; }
            scene = null; return false;
        }
    }

    public static class NarrativeLoader
    {
        public static void Load(string resourcePath)
        {
            // Diagnostic banner to verify this new loader version is executing.
#if NARRATIVE_USE_YAMLDOTNET
            Debug.Log($"[NarrativeLoader] Loading narrative '{resourcePath}' (YamlDotNet ENABLED)");
#else
            Debug.Log($"[NarrativeLoader] Loading narrative '{resourcePath}' (YamlDotNet NOT enabled – expecting JSON only)");
#endif
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"Narrative file not found at Resources/{resourcePath}");
                return;
            }
            var raw = textAsset.text;
            NarrativeRoot root = null;
            if (raw.TrimStart().StartsWith("{"))
            {
                // JSON path
                try { root = JsonUtility.FromJson<NarrativeRoot>(raw); }
                catch (Exception ex) { Debug.LogError($"JSON parse error: {ex.Message}"); }
            }
            else
            {
#if NARRATIVE_USE_YAMLDOTNET
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .IgnoreUnmatchedProperties()
                        .Build();
                    root = deserializer.Deserialize<NarrativeRoot>(raw);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"YamlDotNet parse error: {ex.Message}\n{ex}");
                }
#else
                Debug.LogError("YAML file detected but YamlDotNet not enabled. Add YamlDotNet and define NARRATIVE_USE_YAMLDOTNET in Scripting Define Symbols.");
#endif
            }
            if (root == null)
            {
                Debug.LogError("Failed to parse narrative data (JSON or YAML)");
                return;
            }
            Debug.Log($"[NarrativeLoader] Parsed narrative: speakers={root.speakers?.Count ?? 0}, backgrounds={root.backgrounds?.Count ?? 0}, scenes={root.scenes?.Count ?? 0}, startScene='{root.startScene}'");
            Build(root);
        }

        private static void Build(NarrativeRoot root)
        {
            NarrativeRegistry.StoryScenes.Clear();
            NarrativeRegistry.ChooseScenes.Clear();
            NarrativeRegistry.Backgrounds.Clear();
            NarrativeRegistry.Speakers.Clear();
            NarrativeRegistry.StartSceneId = root.startScene;

            // Backgrounds
            if (root.backgrounds != null)
            {
                foreach (var bg in root.backgrounds)
                {
                    if (string.IsNullOrEmpty(bg.id)) continue;
                    var sprite = string.IsNullOrEmpty(bg.sprite) ? null : Resources.Load<Sprite>(bg.sprite);
                    if (!sprite && !string.IsNullOrEmpty(bg.sprite)) Debug.LogWarning($"Missing background sprite: {bg.sprite}");
                    NarrativeRegistry.Backgrounds[bg.id] = sprite;
                }
            }

            // Speakers
            if (root.speakers != null)
            {
                foreach (var sp in root.speakers)
                {
                    if (string.IsNullOrEmpty(sp.id)) continue;
                    var sr = new SpeakerRuntime
                    {
                        Id = sp.id,
                        Name = sp.name ?? sp.id,
                        Color = ColorFromHex(sp.textColor),
                        Prefab = LoadPrefab(sp.prefab)
                    };
                    if (sp.sprites != null)
                    {
                        foreach (var sPath in sp.sprites)
                        {
                            if (string.IsNullOrEmpty(sPath)) continue;
                            var sprite = Resources.Load<Sprite>(sPath);
                            if (!sprite) Debug.LogWarning($"Missing speaker sprite: {sPath}");
                            sr.Sprites.Add(sprite);
                        }
                    }
                    NarrativeRegistry.Speakers[sp.id] = sr;
                }
            }

            // Scenes
            if (root.scenes != null)
            {
                foreach (var sc in root.scenes)
                {
                    if (string.IsNullOrEmpty(sc.id)) continue;
                    switch (sc.type)
                    {
                        case SceneType.story:
                            NarrativeRegistry.StoryScenes[sc.id] = new RuntimeStoryScene
                            {
                                Id = sc.id,
                                Background = ResolveBackground(sc.background),
                                Sentences = sc.sentences ?? new List<SentenceData>(),
                                NextId = sc.next
                            };
                            break;
                        case SceneType.choice:
                            NarrativeRegistry.ChooseScenes[sc.id] = new RuntimeChooseScene
                            {
                                Id = sc.id,
                                Background = ResolveBackground(sc.background),
                                Choices = sc.choices ?? new List<ChoiceData>()
                            };
                            break;
                        case SceneType.ending:
                            NarrativeRegistry.StoryScenes[sc.id] = new RuntimeStoryScene
                            {
                                Id = sc.id,
                                Background = ResolveBackground(sc.background),
                                Sentences = sc.sentences ?? new List<SentenceData>(),
                                NextId = null
                            };
                            break;
                        default:
                            Debug.LogWarning($"Unknown scene type for {sc.id}, defaulting to story");
                            NarrativeRegistry.StoryScenes[sc.id] = new RuntimeStoryScene
                            {
                                Id = sc.id,
                                Background = ResolveBackground(sc.background),
                                Sentences = sc.sentences ?? new List<SentenceData>(),
                                NextId = sc.next
                            };
                            break;
                    }
                }
            }
        }

        private static Sprite ResolveBackground(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return NarrativeRegistry.Backgrounds.TryGetValue(id, out var s) ? s : null;
        }

        private static Color ColorFromHex(string hex)
        {
            if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }

        private static SpriteController LoadPrefab(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var go = Resources.Load<GameObject>(path);
            if (!go) { Debug.LogWarning($"Missing speaker prefab: {path}"); return null; }
            return go.GetComponent<SpriteController>();
        }
    }
}
