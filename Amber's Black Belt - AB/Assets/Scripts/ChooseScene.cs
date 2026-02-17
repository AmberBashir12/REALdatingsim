using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChooseScene", menuName = "Data/New Choose Scene")]
[System.Serializable]

public class ChooseScene : GameScene
{
    public List<ChooseLabel> labels;

    [System.Serializable]
    public struct ChoiceResult
    {
        public StoryScene nextScene;
        public int startSentenceIndex; // -1 means play from the beginning.
    }

    [System.Serializable]
    public struct ChooseLabel
    {
        public string text;
        public StoryScene nextScene;
        public int startSentenceIndex; // -1 means play from the beginning.
        public string choiceKeyToUnlock; // Optional: choice key to unlock when this option is selected
    }

    public StoryScene GetNextScene(string labelText)
    {
        if (TryGetChoiceResult(labelText, out ChoiceResult result))
        {
            return result.nextScene;
        }

        return null;
    }

    public bool TryGetChoiceResult(string labelText, out ChoiceResult result)
    {
        Debug.Log($"TryGetChoiceResult called with labelText: '{labelText}'");
        result = new ChoiceResult { nextScene = null, startSentenceIndex = -1 };

        // Check regular labels first
        foreach (ChooseLabel label in labels)
        {
            if (label.text == labelText)
            {
                Debug.Log($"Found matching regular label: '{labelText}'");
                // Unlock new choice if specified
                if (!string.IsNullOrEmpty(label.choiceKeyToUnlock) && GameStateManager.Instance != null)
                {
                    Debug.Log($"Unlocking choice key: '{label.choiceKeyToUnlock}'");
                    GameStateManager.Instance.UnlockChoice(label.choiceKeyToUnlock);
                    Debug.Log($"After unlocking, key '{label.choiceKeyToUnlock}' is now unlocked: {GameStateManager.Instance.IsChoiceUnlocked(label.choiceKeyToUnlock)}");
                }
                else if (string.IsNullOrEmpty(label.choiceKeyToUnlock))
                {
                    Debug.Log($"No choice key to unlock for label '{labelText}'");
                }
                else if (GameStateManager.Instance == null)
                {
                    Debug.LogWarning("GameStateManager.Instance is null when trying to unlock choice key!");
                }

                result = new ChoiceResult
                {
                    nextScene = label.nextScene,
                    startSentenceIndex = label.startSentenceIndex
                };
                return true;
            }
        }

        // Check additional labels
        AdditionalLabel? additionalLabel = GetAdditionalLabel(labelText);
        if (additionalLabel.HasValue)
        {
            Debug.Log($"Found matching additional label: '{labelText}'");
            // Unlock new choice if specified
            if (!string.IsNullOrEmpty(additionalLabel.Value.choiceKeyToUnlock) && GameStateManager.Instance != null)
            {
                Debug.Log($"Unlocking choice key: '{additionalLabel.Value.choiceKeyToUnlock}'");
                GameStateManager.Instance.UnlockChoice(additionalLabel.Value.choiceKeyToUnlock);
            }

            result = new ChoiceResult
            {
                nextScene = additionalLabel.Value.nextScene,
                startSentenceIndex = additionalLabel.Value.startSentenceIndex
            };
            return true;
        }

        Debug.LogWarning($"No matching label found for: '{labelText}'");
        return false;
    }

    [System.Serializable]
    public struct AdditionalLabel
    {
        public string text;
        public StoryScene nextScene;
        public int startSentenceIndex; // -1 means play from the beginning.
        public string requiredChoiceKey; // The choice key that must be unlocked to show this option
        public string choiceKeyToUnlock; // Optional: choice key to unlock when this option is selected
    }

    public List<AdditionalLabel> additionalLabels;

    // Get all available choices (base + unlocked additional choices)
    public List<ChooseLabel> GetAvailableChoices()
    {
        List<ChooseLabel> availableChoices = new List<ChooseLabel>(labels);
        Debug.Log($"Base choices count: {labels.Count}");
        
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager.Instance is null!");
            return availableChoices;
        }
        
        if (additionalLabels == null)
        {
            Debug.Log("No additional labels configured for this scene");
            return availableChoices;
        }
        
        Debug.Log($"Checking {additionalLabels.Count} additional labels");
        
        foreach (AdditionalLabel additionalLabel in additionalLabels)
        {
            Debug.Log($"Checking additional choice: '{additionalLabel.text}' - requires key: '{additionalLabel.requiredChoiceKey}'");
            
            if (GameStateManager.Instance.IsChoiceUnlocked(additionalLabel.requiredChoiceKey))
            {
                Debug.Log($"Choice unlocked! Adding: '{additionalLabel.text}'");
                // Convert AdditionalLabel to ChooseLabel
                ChooseLabel newChoice = new ChooseLabel
                {
                    text = additionalLabel.text,
                    nextScene = additionalLabel.nextScene,
                    startSentenceIndex = additionalLabel.startSentenceIndex
                };
                availableChoices.Add(newChoice);
            }
            else
            {
                Debug.Log($"Choice locked. Key '{additionalLabel.requiredChoiceKey}' not unlocked yet.");
            }
        }
        
        Debug.Log($"Total available choices: {availableChoices.Count}");
        return availableChoices;
    }

    // Get additional label by text (used when choice is selected)
    public AdditionalLabel? GetAdditionalLabel(string labelText)
    {
        if (additionalLabels != null)
        {
            foreach (AdditionalLabel label in additionalLabels)
            {
                if (label.text == labelText)
                {
                    return label;
                }
            }
        }
        return null;
    }

}
