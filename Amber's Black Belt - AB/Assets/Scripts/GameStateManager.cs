using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    [SerializeField]
    private HashSet<string> unlockedChoices = new HashSet<string>();
    
    [SerializeField]
    private Dictionary<string, object> gameFlags = new Dictionary<string, object>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to unlock a choice option
    public void UnlockChoice(string choiceKey)
    {
        if (!unlockedChoices.Contains(choiceKey))
        {
            unlockedChoices.Add(choiceKey);
        }
    }

    // Check if a choice is unlocked
    public bool IsChoiceUnlocked(string choiceKey)
    {
        return unlockedChoices.Contains(choiceKey);
    }

    // Set a game flag (can store any data like relationship points, story progress, etc.)
    public void SetFlag(string flagName, object value)
    {
        gameFlags[flagName] = value;
    }

    // Get a game flag
    public T GetFlag<T>(string flagName, T defaultValue = default(T))
    {
        if (gameFlags.ContainsKey(flagName) && gameFlags[flagName] is T)
        {
            return (T)gameFlags[flagName];
        }
        return defaultValue;
    }

    // Check if a flag exists
    public bool HasFlag(string flagName)
    {
        return gameFlags.ContainsKey(flagName);
    }

    // Clear all game state (for new game)
    public void ResetGameState()
    {
        unlockedChoices.Clear();
        gameFlags.Clear();
    }

    // Get all unlocked choices (for debugging)
    public string[] GetUnlockedChoices()
    {
        string[] choices = new string[unlockedChoices.Count];
        unlockedChoices.CopyTo(choices);
        return choices;
    }
}
