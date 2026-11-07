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
        public string choiceKeyToUnlock;
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


        foreach (ChooseLabel label in labels)
        {
            if (label.text == labelText)
            {

                if (!string.IsNullOrEmpty(label.choiceKeyToUnlock) && GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.UnlockChoice(label.choiceKeyToUnlock);
                }

                result = new ChoiceResult
                {
                    nextScene = label.nextScene
                };
                return true;
            }
        }


        AdditionalLabel? additionalLabel = GetAdditionalLabel(labelText);
        if (additionalLabel.HasValue)
        {

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

        return false;
    }

    [System.Serializable]
    public struct AdditionalLabel
    {
        public string text;
        public StoryScene nextScene;
        public string requiredChoiceKey;
        public string choiceKeyToUnlock;
    }

    public List<AdditionalLabel> additionalLabels;


    public List<ChooseLabel> GetAvailableChoices()
    {
        List<ChooseLabel> availableChoices = new List<ChooseLabel>(labels);
        
        if (GameStateManager.Instance == null)
        {
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
