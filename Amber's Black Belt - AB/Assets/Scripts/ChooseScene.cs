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
    }

    [System.Serializable]
    public struct ChooseLabel
    {
        public string text;
        public StoryScene nextScene;
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
        result = new ChoiceResult { nextScene = null };

        // Check regular labels first
        foreach (ChooseLabel label in labels)
        {
            if (label.text == labelText)
            {
                // Unlock new choice if specified
                if (!string.IsNullOrEmpty(label.choiceKeyToUnlock) && GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.UnlockChoice(label.choiceKeyToUnlock);
                }
                else if (GameStateManager.Instance == null)
                {
                    Debug.LogWarning("GameStateManager.Instance is null when trying to unlock choice key!");
                }

                result = new ChoiceResult
                {
                    nextScene = label.nextScene
                };
                return true;
            }
        }

        // Check additional labels
        AdditionalLabel? additionalLabel = GetAdditionalLabel(labelText);
        if (additionalLabel.HasValue)
        {
            // Unlock new choice if specified
            if (!string.IsNullOrEmpty(additionalLabel.Value.choiceKeyToUnlock) && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.UnlockChoice(additionalLabel.Value.choiceKeyToUnlock);
            }

            result = new ChoiceResult
            {
                nextScene = additionalLabel.Value.nextScene
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
        public string requiredChoiceKey; // The choice key that must be unlocked to show this option
        public string choiceKeyToUnlock; // Optional: choice key to unlock when this option is selected
    }

    public List<AdditionalLabel> additionalLabels;

    // Get all available choices (base + unlocked additional choices)
    public List<ChooseLabel> GetAvailableChoices()
    {
        List<ChooseLabel> availableChoices = new List<ChooseLabel>(labels);
        
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager.Instance is null!");
            return availableChoices;
        }
        
        if (additionalLabels == null)
        {
            return availableChoices;
        }
        
        foreach (AdditionalLabel additionalLabel in additionalLabels)
        {
            if (GameStateManager.Instance.IsChoiceUnlocked(additionalLabel.requiredChoiceKey))
            {
                // Convert AdditionalLabel to ChooseLabel
                ChooseLabel newChoice = new ChooseLabel
                {
                    text = additionalLabel.text,
                    nextScene = additionalLabel.nextScene
                };
                availableChoices.Add(newChoice);
            }
        }
        
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
