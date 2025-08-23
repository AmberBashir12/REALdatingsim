#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public static class FullSceneMissingScriptScanner
{
    [MenuItem("Tools/Diagnostics/Scan ALL Scenes For Missing Scripts")] 
    public static void ScanAllScenes()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int totalScenes = sceneGuids.Length;
        int scenesWithMissing = 0;
        int totalMissing = 0;
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Missing Script Report (All Scene Assets) ===");

        // Remember currently open scenes so we can restore
        var openScenes = new List<string>();
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        {
            var s = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
            if (s.path != null) openScenes.Add(s.path);
        }

        // Work list
        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var opened = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            int missingInScene = 0;
            foreach (var root in opened.GetRootGameObjects())
            {
                CountMissingRecursive(root, ref missingInScene, path);
            }
            if (missingInScene > 0)
            {
                scenesWithMissing++;
                totalMissing += missingInScene;
                report.AppendLine($"Scene '{path}' -> Missing components: {missingInScene}");
            }
            // Close scene again (don't save)
            EditorSceneManager.CloseScene(opened, removeScene: true);
        }

        report.AppendLine($"Scenes scanned: {totalScenes}");
        report.AppendLine($"Scenes with missing scripts: {scenesWithMissing}");
        report.AppendLine($"Total missing script components: {totalMissing}");
        if (totalMissing == 0) report.AppendLine("No missing script components in ANY scene asset.");
        Debug.Log(report.ToString());

        // Re-open previously open scenes (single first as main, then additive)
        if (openScenes.Count > 0)
        {
            var main = EditorSceneManager.OpenScene(openScenes[0], OpenSceneMode.Single);
            for (int i = 1; i < openScenes.Count; i++)
            {
                EditorSceneManager.OpenScene(openScenes[i], OpenSceneMode.Additive);
            }
        }
    }

    private static void CountMissingRecursive(GameObject go, ref int missing, string scenePath)
    {
        var comps = go.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null)
            {
                missing++;
            }
        }
        foreach (Transform child in go.transform)
        {
            CountMissingRecursive(child.gameObject, ref missing, scenePath);
        }
    }
}
#endif
