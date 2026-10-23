using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class StorySceneActionProcessor
{
    private const string SceneRoot = "Assets/1Story/Story&ChooseScenes";
    private const string AriaGuid = "aeb00bc743a24afb90dffce6ca9a83f1";
    private const string OldAriaGuid = "d0b7a099455ebe743879cffc04da882b";
    private const string PlayerGuid = "2e240942c73318642b0f986813494e10";

    [MenuItem("Tools/Story/Apply Speaker Actions to Eligible Scenes")]
    public static void ApplyToEligibleScenes()
    {
        RepairAriaData();
        RepairOldAriaReferences();
        candidateCache = null;

        int changedScenes = 0;
        int changedLines = 0;
        HashSet<string> skippedSpeakers = new HashSet<string>();

        foreach (string assetGuid in AssetDatabase.FindAssets("t:StoryScene", new[] { SceneRoot }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            StoryScene scene = AssetDatabase.LoadAssetAtPath<StoryScene>(assetPath);
            if (!IsEligible(scene, assetPath) || scene.sentences == null || scene.sentences.Count == 0)
            {
                continue;
            }

            ProcessScene(scene, assetPath, skippedSpeakers, ref changedLines);
            changedScenes++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Speaker actions applied to {changedScenes} eligible scenes and {changedLines} sentence blocks. Speakers without usable sprite/prefab data were skipped: {string.Join(", ", skippedSpeakers)}");
    }

    // Usage: Unity -batchmode -executeMethod StorySceneActionProcessor.TestScenes -scenePath "Assets/a.asset;Assets/b.asset"
    public static void TestScenes()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-scenePath");
        string[] scenePaths = flag >= 0 && flag + 1 < args.Length ? args[flag + 1].Split(';') : new string[0];

        RepairAriaData();
        candidateCache = null;

        HashSet<string> skippedSpeakers = new HashSet<string>();
        int stagedLines = 0;
        foreach (string scenePath in scenePaths)
        {
            StoryScene scene = AssetDatabase.LoadAssetAtPath<StoryScene>(scenePath);
            if (scene != null && IsEligible(scene, scenePath))
            {
                ProcessScene(scene, scenePath, skippedSpeakers, ref stagedLines);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Test staging of {scenePaths.Length} scenes: {stagedLines} lines. Skipped speakers: {string.Join(", ", skippedSpeakers)}");
    }

    private static void ProcessScene(StoryScene scene, string assetPath, HashSet<string> skippedSpeakers, ref int changedLines)
    {
        List<Speaker> candidates = GetCandidates();

        SceneStager dryRun = new SceneStager(scene, assetPath, candidates, skippedSpeakers, null);
        dryRun.Run();

        SceneStager stager = new SceneStager(scene, assetPath, candidates, skippedSpeakers, GetPositions(dryRun.Cast));
        stager.Run();
        changedLines += stager.StagedLines;

        EditorUtility.SetDirty(scene);
    }

    private static bool IsEligible(StoryScene scene, string assetPath)
    {
        if (scene == null || scene is EndingScene)
        {
            return false;
        }

        return assetPath.IndexOf("/Day1/", StringComparison.OrdinalIgnoreCase) < 0
            && assetPath.IndexOf("/Endings/", StringComparison.OrdinalIgnoreCase) < 0
            && assetPath.IndexOf("/EndingScenes/", StringComparison.OrdinalIgnoreCase) < 0
            && assetPath.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static void RepairAriaData()
    {
        Speaker aria = AssetDatabase.LoadAssetAtPath<Speaker>("Assets/1Story/Speakers/Aria.asset");
        GameObject spritePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/1Story/Charcters/Eleanor/Eleanor.prefab");
        if (aria == null || spritePrefab == null)
        {
            Debug.LogWarning("Aria's sprite data or the shared character prefab could not be found.");
            return;
        }

        string[] spritePaths =
        {
            "Assets/1Story/Charcters/Eleanor/Aria Normal.png",
            "Assets/1Story/Charcters/Eleanor/Aria Normal Talk.png",
            "Assets/1Story/Charcters/Eleanor/Aria Normal Smile.png",
            "Assets/1Story/Charcters/Eleanor/Aria Normal smile Talk.png",
            "Assets/1Story/Charcters/Eleanor/Aria Happy.png",
            "Assets/1Story/Charcters/Eleanor/Aria Happy Talk.png",
            "Assets/1Story/Charcters/Eleanor/Aria Annoyed.png",
            "Assets/1Story/Charcters/Eleanor/Aria Annoyed Talk.png",
            "Assets/1Story/Charcters/Eleanor/Aria Cheerful talk.png"
        };

        List<Sprite> sprites = spritePaths
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
            .Where(sprite => sprite != null)
            .ToList();

        if (sprites.Count > 0)
        {
            aria.sprites = sprites;
            aria.prefab = spritePrefab.GetComponent<SpriteController>();
            EditorUtility.SetDirty(aria);
        }
    }

    private static void RepairOldAriaReferences()
    {
        foreach (string assetGuid in AssetDatabase.FindAssets("t:StoryScene", new[] { SceneRoot }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            StoryScene scene = AssetDatabase.LoadAssetAtPath<StoryScene>(assetPath);
            if (scene == null
                || scene is EndingScene
                || assetPath.IndexOf("/Endings/", StringComparison.OrdinalIgnoreCase) >= 0
                || assetPath.IndexOf("/EndingScenes/", StringComparison.OrdinalIgnoreCase) >= 0
                || assetPath.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            string contents = File.ReadAllText(assetPath);
            if (!contents.Contains(OldAriaGuid))
            {
                continue;
            }

            File.WriteAllText(assetPath, contents.Replace(OldAriaGuid, AriaGuid));
            AssetDatabase.ImportAsset(assetPath);
        }
    }

    private static StoryScene.Sentence.Action CreateAction(Speaker speaker, int spriteIndex, StoryScene.Sentence.Action.Type type, Vector2 position)
    {
        return new StoryScene.Sentence.Action
        {
            speaker = speaker,
            spriteIndex = spriteIndex,
            actionType = type,
            coords = position,
            moveSpeed = 0f,
            tintColor = Color.clear,
            tintOpacity = 0f
        };
    }

    private static void GetSpriteIndices(Speaker speaker, string assetPath, out int neutralIndex, out int talkIndex)
    {
        neutralIndex = 0;
        talkIndex = 0;
        if (speaker == null || speaker.sprites == null || speaker.sprites.Count == 0)
        {
            return;
        }

        if (AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(speaker)) == PlayerGuid)
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            if (normalizedPath.IndexOf("/Sleep In/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                neutralIndex = 6;
                talkIndex = 7;
            }
            else if (normalizedPath.IndexOf("/Day2/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalizedPath.IndexOf("/Day3/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalizedPath.IndexOf("/Day4/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalizedPath.IndexOf("/Day5/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalizedPath.IndexOf("/Day6/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                neutralIndex = 12;
                talkIndex = 13;
            }
            else
            {
                neutralIndex = 6;
                talkIndex = 7;
            }

            if (neutralIndex >= speaker.sprites.Count || speaker.sprites[neutralIndex] == null)
            {
                neutralIndex = 0;
            }
            if (talkIndex >= speaker.sprites.Count || speaker.sprites[talkIndex] == null)
            {
                talkIndex = neutralIndex;
            }
            return;
        }

        int firstValidIndex = speaker.sprites.FindIndex(sprite => sprite != null);
        if (firstValidIndex < 0)
        {
            return;
        }

        int normalIndex = speaker.sprites.FindIndex(sprite => sprite != null
            && NormalizeSpriteName(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite))).Contains("normal")
            && !NormalizeSpriteName(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite))).Contains("talk"));
        neutralIndex = normalIndex >= 0 ? normalIndex : firstValidIndex;

        string neutralName = NormalizeSpriteName(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(speaker.sprites[neutralIndex])));
        int matchingTalkIndex = -1;
        int firstTalkIndex = -1;
        for (int i = 0; i < speaker.sprites.Count; i++)
        {
            Sprite sprite = speaker.sprites[i];
            if (sprite == null)
            {
                continue;
            }

            string spriteName = NormalizeSpriteName(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite)));
            if (!spriteName.Contains("talk"))
            {
                continue;
            }

            if (firstTalkIndex < 0)
            {
                firstTalkIndex = i;
            }

            string talkBaseName = spriteName.Replace("talk", string.Empty);
            if (talkBaseName == neutralName || neutralName.StartsWith(talkBaseName) || talkBaseName.StartsWith(neutralName))
            {
                matchingTalkIndex = i;
                break;
            }
        }

        talkIndex = matchingTalkIndex >= 0 ? matchingTalkIndex : (firstTalkIndex >= 0 ? firstTalkIndex : neutralIndex);
    }

    private static string NormalizeSpriteName(string name)
    {
        return new string(name.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    }
}
