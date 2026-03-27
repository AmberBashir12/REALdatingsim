using UnityEngine;

public class GameStateSetup : MonoBehaviour
{
    [Header("Initial Setup")]
    [SerializeField] private bool resetStateOnStart = false;
    
    [Header("Pre-unlock Choices (for testing)")]
    [SerializeField] private string[] choicesToUnlock;
    
    private void Start()
    {
        // Ensure GameStateManager exists
        if (GameStateManager.Instance == null)
        {
            GameObject gameStateObject = new GameObject("GameStateManager");
            gameStateObject.AddComponent<GameStateManager>();
        }
        
        if (resetStateOnStart)
        {
            GameStateManager.Instance.ResetGameState();
        }
        
        // Unlock any pre-configured choices (useful for testing)
        foreach (string choiceKey in choicesToUnlock)
        {
            if (!string.IsNullOrEmpty(choiceKey))
            {
                GameStateManager.Instance.UnlockChoice(choiceKey);
            }
        }
    }
}
