using UnityEngine;

[System.Serializable]
public class ChoiceSystemHelper : MonoBehaviour
{
    [Header("Choice System Utilities")]
    [SerializeField] private string[] debugUnlockedChoices;
    
    [Header("Actions")]
    [SerializeField] private string choiceToUnlock;
    [SerializeField] private string choiceToCheck;
    
    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            debugUnlockedChoices = GameStateManager.Instance.GetUnlockedChoices();
        }
    }
    
    [ContextMenu("Unlock Choice")]
    private void UnlockChoice()
    {
        if (!string.IsNullOrEmpty(choiceToUnlock) && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockChoice(choiceToUnlock);
            Debug.Log($"Unlocked choice: {choiceToUnlock}");
            RefreshDebugInfo();
        }
    }
    
    [ContextMenu("Check Choice Status")]
    private void CheckChoice()
    {
        if (!string.IsNullOrEmpty(choiceToCheck) && GameStateManager.Instance != null)
        {
            bool isUnlocked = GameStateManager.Instance.IsChoiceUnlocked(choiceToCheck);
            Debug.Log($"Choice '{choiceToCheck}' is {(isUnlocked ? "UNLOCKED" : "LOCKED")}");
        }
    }
    
    [ContextMenu("Reset All Choices")]
    private void ResetChoices()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetGameState();
            Debug.Log("All choices reset");
            RefreshDebugInfo();
        }
    }
    
    [ContextMenu("Refresh Debug Info")]
    private void RefreshDebugInfo()
    {
        if (GameStateManager.Instance != null)
        {
            debugUnlockedChoices = GameStateManager.Instance.GetUnlockedChoices();
        }
    }
}
