#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class MissingScriptScanner
{
    [MenuItem("Tools/Diagnostics/List Missing Scripts (Scenes + Prefabs)")]
    public static void ListMissingScripts()
    {
        int goCount = 0;
        int componentsCount = 0;
        int missingCount = 0;
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Missing Script Report ===");

        // Open scenes currently loaded
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (var root in scene.GetRootGameObjects())
            {
                ScanGameObject(root, ref goCount, ref componentsCount, ref missingCount, report, context:$"(Scene:{scene.name})");
            }
        }

        // Prefabs in project
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            ScanGameObject(prefab, ref goCount, ref componentsCount, ref missingCount, report, context:$"(Prefab:{path})");
        }

        report.AppendLine($"Scanned GameObjects: {goCount}");
        report.AppendLine($"Scanned Components: {componentsCount}");
        report.AppendLine($"Missing Components: {missingCount}");
        Debug.Log(report.ToString());
        if (missingCount == 0) Debug.Log("No missing scripts detected.");
    }

    [MenuItem("Tools/Diagnostics/Remove Missing Scripts In Loaded Scenes")]
    public static void RemoveMissingInScenes()
    {
        int removed = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (var root in scene.GetRootGameObjects())
            {
                removed += RemoveMissingRecursive(root);
            }
        }
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} missing script component(s) from loaded scenes.");
        }
        else Debug.Log("No missing script components found to remove in loaded scenes.");
    }

    private static void ScanGameObject(GameObject go, ref int goCount, ref int compCount, ref int missingCount, System.Text.StringBuilder report, string context)
    {
        goCount++;
        var comps = go.GetComponents<Component>();
        compCount += comps.Length;
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null)
            {
                missingCount++;
                report.AppendLine($"Missing script on GameObject '{GetPath(go)}' {context}");
            }
        }
        foreach (Transform child in go.transform)
        {
            ScanGameObject(child.gameObject, ref goCount, ref compCount, ref missingCount, report, context);
        }
    }

    private static int RemoveMissingRecursive(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
        {
            removed += RemoveMissingRecursive(child.gameObject);
        }
        return removed;
    }

    private static string GetPath(GameObject go)
    {
        if (go.transform.parent == null) return go.name;
        return GetPath(go.transform.parent.gameObject) + "/" + go.name;
    }
}
#endif
