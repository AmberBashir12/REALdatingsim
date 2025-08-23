using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Narrative;
using System.Collections.Generic;

// Exports existing StoryScene, ChooseScene, and Speaker ScriptableObjects into a single YAML file matching NarrativeRoot schema.
public static class ExportScriptableObjectsToYaml
{
#if UNITY_EDITOR
    [MenuItem("Tools/Narrative/Export ScriptableObjects To YAML")] 
    public static void ExportAll()
    {
        var storyScenes = LoadAssets<StoryScene>();
        var chooseScenes = LoadAssets<ChooseScene>();
        var speakers = LoadAssets<Speaker>();

        var root = new NarrativeRoot
        {
            startScene = GuessStartSceneId(storyScenes, chooseScenes),
            backgrounds = new List<BackgroundData>(),
            speakers = new List<SpeakerData>(),
            scenes = new List<SceneData>()
        };

        // Speakers
        foreach (var sp in speakers)
        {
            var sd = new SpeakerData
            {
                id = SanitizeId(sp.name),
                name = sp.speakerName,
                textColor = ColorUtility.ToHtmlStringRGBA(sp.textColor).Insert(0, "#"),
                prefab = GetResourcesPath(sp.prefab ? sp.prefab.gameObject : null),
                sprites = new List<string>()
            };
            if (sp.sprites != null)
            {
                foreach (var sprite in sp.sprites)
                {
                    sd.sprites.Add(GetResourcesPath(sprite));
                }
            }
            root.speakers.Add(sd);
        }

        // Scenes (Story)
        foreach (var sc in storyScenes)
        {
            var sceneData = new SceneData
            {
                id = SanitizeId(sc.name),
                type = SceneType.story,
                background = GetResourcesPath(sc.background),
                sentences = new List<SentenceData>(),
                next = sc.nextScene ? SanitizeId(sc.nextScene.name) : null
            };
            if (sc.sentences != null)
            {
                foreach (var s in sc.sentences)
                {
                    var sen = new SentenceData
                    {
                        speaker = s.speaker ? SanitizeId(s.speaker.name) : null,
                        text = s.text,
                        actions = new List<ActionData>()
                    };
                    if (s.actions != null)
                    {
                        foreach (var a in s.actions)
                        {
                            var act = new ActionData
                            {
                                speaker = a.speaker ? SanitizeId(a.speaker.name) : null,
                                type = (ActionType)System.Enum.Parse(typeof(ActionType), a.actionType.ToString(), true),
                                spriteIndex = a.spriteIndex,
                                x = a.coords.x,
                                y = a.coords.y,
                                moveSpeed = a.moveSpeed
                            };
                            sen.actions.Add(act);
                        }
                    }
                    sceneData.sentences.Add(sen);
                }
            }
            root.scenes.Add(sceneData);
        }

        // Scenes (Choose)
        foreach (var cc in chooseScenes)
        {
            var sceneData = new SceneData
            {
                id = SanitizeId(cc.name),
                type = SceneType.choice,
                background = null,
                choices = new List<ChoiceData>()
            };
            if (cc.labels != null)
            {
                foreach (var l in cc.labels)
                {
                    var ch = new ChoiceData
                    {
                        text = l.text,
                        next = l.nextScene ? SanitizeId(l.nextScene.name) : null
                    };
                    sceneData.choices.Add(ch);
                }
            }
            root.scenes.Add(sceneData);
        }

        var yaml = BuildYaml(root);
        var outPath = Path.Combine(Application.dataPath, "Resources/Narrative/exported_narrative.yaml");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, yaml, Encoding.UTF8);
        Debug.Log($"Exported narrative YAML to {outPath}\n---\n{yaml.Substring(0, Mathf.Min(500, yaml.Length))}...");
        AssetDatabase.Refresh();
    }

    private static string GuessStartSceneId(StoryScene[] storyScenes, ChooseScene[] chooseScenes)
    {
        // Heuristic: pick a StoryScene that is not referenced as nextScene by any other scene nor by a choice.
        var referenced = new HashSet<StoryScene>();
        foreach (var sc in storyScenes)
        {
            if (sc.nextScene) referenced.Add(sc.nextScene as StoryScene);
        }
        foreach (var cc in chooseScenes)
        {
            if (cc.labels != null)
                foreach (var l in cc.labels)
                    if (l.nextScene) referenced.Add(l.nextScene);
        }
        foreach (var sc in storyScenes)
        {
            if (!referenced.Contains(sc)) return SanitizeId(sc.name);
        }
        return storyScenes.Length > 0 ? SanitizeId(storyScenes[0].name) : "start";
    }

    private static string BuildYaml(NarrativeRoot root)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"startScene: {root.startScene}");
        sb.AppendLine("backgrounds:");
        foreach (var bg in root.backgrounds)
        {
            sb.AppendLine("  - id: " + bg.id);
            sb.AppendLine("    sprite: " + bg.sprite);
        }
        sb.AppendLine("speakers:");
        foreach (var sp in root.speakers)
        {
            sb.AppendLine("  - id: " + sp.id);
            sb.AppendLine("    name: \"" + Escape(sp.name) + "\"");
            sb.AppendLine("    textColor: \"" + sp.textColor + "\"");
            if (!string.IsNullOrEmpty(sp.prefab)) sb.AppendLine("    prefab: " + sp.prefab);
            foreach (var spr in sp.sprites) sb.AppendLine("    sprite: " + spr);
        }
        sb.AppendLine("scenes:");
        foreach (var sc in root.scenes)
        {
            sb.AppendLine("  - id: " + sc.id);
            sb.AppendLine("    type: " + sc.type.ToString());
            if (!string.IsNullOrEmpty(sc.background)) sb.AppendLine("    background: " + sc.background);
            if (sc.sentences != null && sc.sentences.Count > 0)
            {
                sb.AppendLine("    sentences:");
                foreach (var sen in sc.sentences)
                {
                    sb.Append("      - text: \"" + Escape(sen.text) + "\"\n");
                    if (!string.IsNullOrEmpty(sen.speaker)) sb.AppendLine("        speaker: " + sen.speaker);
                    if (sen.actions != null && sen.actions.Count > 0)
                    {
                        sb.AppendLine("        actions:");
                        foreach (var act in sen.actions)
                        {
                            sb.AppendLine("          - speaker: " + act.speaker);
                            sb.AppendLine("            type: " + act.type.ToString());
                            if (act.spriteIndex != 0) sb.AppendLine("            spriteIndex: " + act.spriteIndex);
                            if (act.x != 0) sb.AppendLine("            x: " + act.x.ToString("0.##"));
                            if (act.y != 0) sb.AppendLine("            y: " + act.y.ToString("0.##"));
                            if (act.moveSpeed != 0) sb.AppendLine("            moveSpeed: " + act.moveSpeed.ToString("0.##"));
                        }
                    }
                }
            }
            if (sc.choices != null && sc.choices.Count > 0)
            {
                sb.AppendLine("    choices:");
                foreach (var ch in sc.choices)
                {
                    sb.AppendLine("      - text: \"" + Escape(ch.text) + "\"");
                    if (!string.IsNullOrEmpty(ch.next)) sb.AppendLine("        next: " + ch.next);
                }
            }
            if (!string.IsNullOrEmpty(sc.next)) sb.AppendLine("    next: " + sc.next);
        }
        return sb.ToString();
    }

    private static string Escape(string input)
    {
        return input?.Replace("\"", "\\\"") ?? string.Empty;
    }

    private static string SanitizeId(string name)
    {
        if (string.IsNullOrEmpty(name)) return "id";
        var sb = new StringBuilder();
        foreach (var ch in name.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_') sb.Append(ch);
            else if (ch == ' ') sb.Append('_');
        }
        if (sb.Length == 0) sb.Append("id");
        return sb.ToString();
    }

    private static T[] LoadAssets<T>() where T : UnityEngine.Object
    {
        var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        var list = new List<T>();
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) list.Add(asset);
        }
        return list.ToArray();
    }

    private static string GetResourcesPath(Object obj)
    {
        if (!obj) return null;
        var path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return null;
        var resourcesIndex = path.ToLowerInvariant().IndexOf("resources/");
        if (resourcesIndex < 0) return null; // Not under Resources
        var rel = path.Substring(resourcesIndex + 10); // after 'resources/'
        if (rel.EndsWith(".prefab")) rel = rel.Substring(0, rel.Length - 7);
        if (rel.EndsWith(".png")) rel = rel.Substring(0, rel.Length - 4);
        return rel;
    }
#endif
}
