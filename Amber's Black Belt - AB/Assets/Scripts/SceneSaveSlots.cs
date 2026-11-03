using System;
using UnityEngine;

[Serializable]
public class SceneSlotData
{
    public int version = 1;
    public string sceneId;
    public string backgroundSceneId;
    public string playerName;
    public string characterChoice;
    public string[] choices;
}

public static class SceneSaveSlots
{
    public const int Count = 4;
    private const string Prefix = "SceneSaveSlots.";
    private static SceneSlotData pendingLoad;

    public static bool TryRead(int slot, out SceneSlotData data)
    {
        data = null;
        if (slot < 0 || slot >= Count)
        {
            Debug.LogError($"Invalid save slot: {slot}.");
            return false;
        }

        if (!PlayerPrefs.HasKey(Prefix + slot))
        {
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<SceneSlotData>(PlayerPrefs.GetString(Prefix + slot));
        }
        catch (ArgumentException exception)
        {
            Debug.LogError($"Save slot {slot + 1} is corrupt: {exception.Message}");
            return false;
        }

        if (data == null || data.version != 1 || string.IsNullOrEmpty(data.sceneId))
        {
            Debug.LogError($"Save slot {slot + 1} is invalid or unsupported.");
            data = null;
            return false;
        }

        return true;
    }

    public static bool Write(int slot, SceneSlotData data)
    {
        if (slot < 0 || slot >= Count || data == null || string.IsNullOrEmpty(data.sceneId))
        {
            Debug.LogError("Cannot write an invalid scene save.");
            return false;
        }

        PlayerPrefs.SetString(Prefix + slot, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        return true;
    }

    public static void QueueLoad(SceneSlotData data) => pendingLoad = data;

    public static SceneSlotData TakePendingLoad()
    {
        SceneSlotData data = pendingLoad;
        pendingLoad = null;
        return data;
    }
}
