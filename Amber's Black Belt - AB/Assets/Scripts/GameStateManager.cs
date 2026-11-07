using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    [SerializeField]
    private HashSet<string> unlockedChoices = new HashSet<string>();

    private void Awake()
    {

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


    public void UnlockChoice(string choiceKey)
    {
        if (!unlockedChoices.Contains(choiceKey))
        {
            unlockedChoices.Add(choiceKey);
        }
    }


    public bool IsChoiceUnlocked(string choiceKey)
    {
        return unlockedChoices.Contains(choiceKey);
    }


    public void ResetGameState()
    {
        unlockedChoices.Clear();
    }


    public string[] GetUnlockedChoices()
    {
        string[] choices = new string[unlockedChoices.Count];
        unlockedChoices.CopyTo(choices);
        return choices;
    }
}
