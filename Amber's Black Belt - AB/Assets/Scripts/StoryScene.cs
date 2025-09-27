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
        public Speaker speaker;
        public string text;
        public List<Action> actions;

        public AudioClip music;
        public AudioClip sound;

        [System.Serializable]
        public struct Action
        {
            public Speaker speaker;
            public int spriteIndex;
            public Type actionType;
            public Vector2 coords;
            public float moveSpeed;

            [System.Serializable]

            public enum Type
            {
                NONE, APPEAR, MOVE, DISAPPEAR, BOUNCE
            }
        }
    }
    [System.Serializable]
    public struct AlternativeScenes
    {
        public string requiredChoiceKey; // Key that must be unlocked to access this alternative scene
        public StoryScene alternativeScene;
    }

    // Get the next scene, checking for unlocked alternatives first
    public GameScene GetNextScene()
    {
        Debug.Log($"GetNextScene called on StoryScene: {name}");
        Debug.Log($"Default nextScene: {(nextScene != null ? nextScene.name : "NULL")}");
        
        // Check if any alternative scenes should be used instead
        if (alternativeScenes == null)
        {
            Debug.Log("alternativeScenes is null");
        }
        else if (alternativeScenes.Count == 0)
        {
            Debug.Log("alternativeScenes list is empty");
        }
        else
        {
            Debug.Log($"Found {alternativeScenes.Count} alternative scenes to check");
        }
        
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager.Instance is null!");
        }
        
        if (alternativeScenes != null && GameStateManager.Instance != null)
        {
            for (int i = 0; i < alternativeScenes.Count; i++)
            {
                AlternativeScenes altScene = alternativeScenes[i];
                Debug.Log($"Checking alternative scene {i}: requiredKey='{altScene.requiredChoiceKey}', scene={altScene.alternativeScene?.name ?? "NULL"}");
                
                if (string.IsNullOrEmpty(altScene.requiredChoiceKey))
                {
                    Debug.Log($"Alternative scene {i} has empty/null requiredChoiceKey");
                }
                else
                {
                    bool isUnlocked = GameStateManager.Instance.IsChoiceUnlocked(altScene.requiredChoiceKey);
                    Debug.Log($"Choice key '{altScene.requiredChoiceKey}' is unlocked: {isUnlocked}");
                    
                    if (isUnlocked)
                    {
                        Debug.Log($"Using alternative scene for unlocked key: '{altScene.requiredChoiceKey}' -> {altScene.alternativeScene?.name ?? "NULL"}");
                        return altScene.alternativeScene;
                    }
                }
            }
        }
        
        // No alternative scene found, use the default next scene
        Debug.Log($"No alternative scenes found, using default nextScene: {(nextScene != null ? nextScene.name : "NULL")}");
        return nextScene;
    }
}

public class GameScene : ScriptableObject { }


