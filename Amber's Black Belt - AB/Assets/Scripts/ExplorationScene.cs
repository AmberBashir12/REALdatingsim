using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewExplorationScene", menuName = "Data/New Exploration Scene")]
[System.Serializable]
public class ExplorationScene : GameScene
{
    [Header("Scene Setup")]
    public Sprite background;
    public List<AudioClip> audioClips;
    
    [Header("Interactive Elements")]
    public List<InteractiveSpeaker> speakers;
    public List<InteractiveObject> interactableObjects;
    
    [System.Serializable]
    public struct InteractiveSpeaker
    {
        public Speaker speaker;
        [Tooltip("Position in canvas coordinates (like story scenes)")]
        public Vector2 coords;
        [TextArea(3, 5)]
        public string dialogueText;
    }
    
    [System.Serializable]
    public struct InteractiveObject
    {
        [Tooltip("Prefab to instantiate for this interactive object")]
        public GameObject objectPrefab;
        [Tooltip("Position in canvas coordinates (like story scenes)")]
        public Vector2 coords;
        public GameScene nextScene;
        public AudioClip soundOnClick;
        [Tooltip("Optional: Choice key required to interact with this object")]
        public string requiredChoiceKey;
        [Tooltip("Optional: Choice key to unlock when this object is clicked")]
        public string choiceKeyToUnlock;
    }
}
