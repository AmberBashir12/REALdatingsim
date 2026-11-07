using UnityEngine;

public class GameStateSetup : MonoBehaviour
{
    [Header("Initial Setup")]
    [SerializeField] private bool resetStateOnStart = false;
    
    [Header("Pre-unlock Choices (for testing)")]
    [SerializeField] private string[] choicesToUnlock;
    
    private void Start()
    {

        if (GameStateManager.Instance == null)
        {
            GameObject gameStateObject = new GameObject("GameStateManager");
            gameStateObject.AddComponent<GameStateManager>();
        }
        
        if (resetStateOnStart)
        {
            GameStateManager.Instance.ResetGameState();
        }
        

        foreach (string choiceKey in choicesToUnlock)
        {
            if (!string.IsNullOrEmpty(choiceKey))
            {
                GameStateManager.Instance.UnlockChoice(choiceKey);
            }
        }
    }
}
