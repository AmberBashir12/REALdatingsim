using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewStoryScene", menuName = "Data/New Story Scene")]
[System.Serializable]
public class StoryScene : GameScene
{
    public List<Sentence> sentences;
    public Sprite background;
    public GameScene nextScene;
    public List<AlternativeScenes> alternativeScenes;
    
    [System.Serializable]
    public struct Sentence
    {
        public SentenceType sentenceType;
        public Speaker speaker;
        public string text;
        public List<Action> actions;
        public ChoiceBlock choice;

        public EndingScene endingScene;

        public AudioClip music;
        public AudioClip music2;
        public AudioClip sound;

        [System.Serializable]
        public struct Action
        {
            public Speaker speaker;
            public int spriteIndex;
            public Type actionType;
            public Vector2 coords;
            public float moveSpeed;
            public Color tintColor;
            [Range(0f, 1f)] public float tintOpacity;

            [System.Serializable]

            public enum Type
            {
                NONE, APPEAR, MOVE, DISAPPEAR, BOUNCE
            }
        }

        [System.Serializable]
        public struct ChoiceBlock
        {
            public string prompt;
            public List<ChoiceOption> options;
        }

        [System.Serializable]
        public struct ChoiceOption
        {
            public string text;
            public List<string> requiredChoiceKeys;
            public string choiceKeyToUnlock;
            public List<FollowUpSentence> followUpSentences;
        }

        public enum SentenceType
        {
            SENTENCE,
            CHOICE,
            ENDING
        }
    }

    [System.Serializable]
    public struct FollowUpSentence
    {
        public FollowUpType followUpType;
        public Speaker speaker;
        public string text;
        public List<Sentence.Action> actions;
        public AudioClip music;
        public AudioClip music2;
        public AudioClip sound;
        public FollowUpChoice choice;
        public EndingScene endingScene;

        public enum FollowUpType
        {
            SENTENCE,
            CHOICE,
            ENDING
        }
    }

    [System.Serializable]
    public struct FollowUpChoice
    {
        public string prompt;
        public List<FollowUpChoiceOption> options;
    }

    [System.Serializable]
    public struct FollowUpChoiceOption
    {
        public string text;
        public List<string> requiredChoiceKeys;
        public string choiceKeyToUnlock;
        public List<FollowUpLine> followUpLines;
    }

    [System.Serializable]
    public struct FollowUpLine
    {
        public LineType lineType;
        public Speaker speaker;
        public string text;
        public List<Sentence.Action> actions;
        public AudioClip music;
        public AudioClip music2;
        public AudioClip sound;
        public EndingScene endingScene;

        public enum LineType
        {
            SENTENCE,
            ENDING
        }
    }

    [System.Serializable]
    public struct AlternativeScenes
    {
        public List<string> requiredChoiceKeys; // All keys that must be unlocked to access this alternative scene
        public StoryScene alternativeScene;
    }

    // Get the next scene, checking for unlocked alternatives first
    public GameScene GetNextScene()
    {
        if (alternativeScenes != null && GameStateManager.Instance != null)
        {
            for (int i = 0; i < alternativeScenes.Count; i++)
            {
                AlternativeScenes altScene = alternativeScenes[i];
                
                if (altScene.requiredChoiceKeys == null || altScene.requiredChoiceKeys.Count == 0)
                {
                    continue;
                }
                
                bool allKeysUnlocked = true;
                
                foreach (string requiredKey in altScene.requiredChoiceKeys)
                {
                    if (string.IsNullOrEmpty(requiredKey))
                    {
                        continue;
                    }
                    
                    bool isUnlocked = GameStateManager.Instance.IsChoiceUnlocked(requiredKey);
                    
                    if (!isUnlocked)
                    {
                        allKeysUnlocked = false;
                        break;
                    }
                }

                if (allKeysUnlocked)
                {
                    return altScene.alternativeScene;
                }
            }
        }
        
        // No alternative scene found, use the default next scene
        return nextScene;
    }
}

public class GameScene : ScriptableObject { }


