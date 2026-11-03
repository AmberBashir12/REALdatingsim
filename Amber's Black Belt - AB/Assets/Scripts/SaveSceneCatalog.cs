using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveSceneCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string id;
        public GameScene scene;

        public Sprite Background => scene is StoryScene story ? story.background
            : scene is ExplorationScene exploration ? exploration.background : null;
    }

    [Tooltip("Stable scene asset GUIDs. After adding scenes, use Refresh Scene Catalog from this asset's context menu.")]
    public List<Entry> entries = new List<Entry>();

    public Entry Find(string id) => entries.Find(entry => entry.scene != null && entry.id == id);
    public Entry Find(GameScene scene) => entries.Find(entry => entry.scene != null && entry.scene == scene);
    public Entry FindBackground(Sprite sprite) => entries.Find(entry => entry.Background == sprite);

#if UNITY_EDITOR
    [ContextMenu("Refresh Scene Catalog")]
    private void RefreshSceneCatalog()
    {
        entries.Clear();
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:GameScene", new[] { "Assets/1Story" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameScene scene = UnityEditor.AssetDatabase.LoadAssetAtPath<GameScene>(path);
            if (scene != null)
            {
                entries.Add(new Entry { id = guid, scene = scene });
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
}
