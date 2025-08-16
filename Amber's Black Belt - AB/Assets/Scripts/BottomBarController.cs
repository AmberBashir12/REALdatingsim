using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using System;

public class BottomBarController : MonoBehaviour
{
    public TextMeshProUGUI barText;
    public TextMeshProUGUI personNameText;

    private int sentenceIndex = -1;
    public StoryScene currentScene;
    private State state = State.COMPLETED;
    private Animator animator;
    public bool IsHidden = false;

    public Dictionary<Speaker, SpriteController> sprites = new Dictionary<Speaker, SpriteController>();

    // public Speaker[] Speakers;
    // public SpriteController[] SpriteControllers;
    public GameObject spritesPrefab;

    private enum State
    {
        PLAYING, COMPLETED
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Hide()
    {
        if (!IsHidden)
        {
            animator.SetTrigger("Hide");
            IsHidden = true;
        }

    }

    public void Show()
    {
        if (IsHidden)
        {
            animator.SetTrigger("Show");
            IsHidden = false;
        }
    }

    public void Bounce()
    {
        animator.SetTrigger("Bounce");
    }

    public void ClearText()
    {
        barText.text = "";
    }

    public void PlayScene(StoryScene scene)
    {
        currentScene = scene;
        sentenceIndex = -1;

        if (currentScene == null)
        {
            Debug.LogError("BottomBarController.PlayScene was called with a null scene.");
            barText.text = "Error: Scene data is missing.";
            personNameText.text = "";
            state = State.COMPLETED; // Mark as completed to avoid getting stuck
            return;
        }

        if (currentScene.sentences == null || currentScene.sentences.Count == 0)
        {
            Debug.LogWarning($"StoryScene '{currentScene.name}' has no sentences. Marking as completed.");
            barText.text = ""; 
            personNameText.text = ""; 
            state = State.COMPLETED; // If no sentences, it's immediately completed.
                                     // GameController will then check IsLastSentence.
        }
        else
        {
            PlayNextSentence();
        }
    }

    public void PlayNextSentence()
    {
        // Ensure currentScene and its sentences are valid before proceeding
        if (currentScene == null || currentScene.sentences == null || currentScene.sentences.Count == 0)
        {
            Debug.LogError("PlayNextSentence called, but currentScene is null or has no sentences.");
            state = State.COMPLETED; // Mark as completed to prevent getting stuck
            return;
        }

        // Check if we are trying to play beyond the last sentence
        if (sentenceIndex + 1 >= currentScene.sentences.Count)
        {
            Debug.LogWarning("PlayNextSentence called, but already at/past the last sentence. This should be handled by GameController.");
            state = State.COMPLETED; // Ensure state is COMPLETED
            return;
        }

        sentenceIndex++; // Increment sentenceIndex *before* using it

        StartCoroutine(TypeText(currentScene.sentences[sentenceIndex].text));
        
        Speaker speaker = currentScene.sentences[sentenceIndex].speaker;
        if (speaker != null)
        {
            personNameText.text = speaker.speakerName;
            personNameText.color = speaker.textColor;
        }
        else
        {
            personNameText.text = ""; // Clear name if no speaker
            // Optional: Log a warning if a speaker is expected but is null for this sentence
            // Debug.LogWarning($"Sentence {sentenceIndex} in scene '{currentScene.name}' has a null speaker.");
        }
        ActSpeakers();
    }

    public bool IsCompleted()
    {
        return state == State.COMPLETED;
    }

    public bool IsLastSentence()
    {
        if (currentScene == null || currentScene.sentences == null)
        {
            // If scene or sentences are null, consider it as if there are no more sentences.
            return true; 
        }
        return sentenceIndex + 1 >= currentScene.sentences.Count;
    }

    private IEnumerator TypeText(string text)
    {
        barText.text = "";
        state = State.PLAYING;
        int wordIndex = 0;

        float normalDelay = 0.05f;
        float fastDelay = 0.01f; // Adjust as desired

        while (wordIndex < text.Length)
        {
            barText.text += text[wordIndex];
            // If spacebar is held, use fastDelay, otherwise use normalDelay
            float delay = Input.GetKey(KeyCode.Space) ? fastDelay : normalDelay;
            yield return new WaitForSeconds(delay);
            wordIndex++;
        }
        
        state = State.COMPLETED;
    }

    private void ActSpeakers()
    {
        List<StoryScene.Sentence.Action> actions = currentScene.sentences[sentenceIndex].actions;
        for (int i = 0; i < actions.Count; i++)
        {
            Debug.Log(i);
            ActSpeaker(actions[i]);
        }
    }

    private void ActSpeaker(StoryScene.Sentence.Action action)
    {
        SpriteController controller = null;
        switch (action.actionType)
        {
            case StoryScene.Sentence.Action.Type.APPEAR:
                try
                {
                    if (!sprites.ContainsKey(action.speaker))
                    {
                        controller = Instantiate(action.speaker.prefab.gameObject, spritesPrefab.transform)
                            .GetComponent<SpriteController>();
                        sprites.Add(action.speaker, controller);
                    }
                    else
                    {
                        controller = sprites[action.speaker];
                    }
                    Debug.Log($"Speaker: {action.speaker}, Sprites: {action.speaker.sprites}, Index: {action.spriteIndex}");
                    if (action.speaker.sprites == null || action.speaker.sprites.Count <= action.spriteIndex || action.speaker.sprites[action.spriteIndex] == null)
                    {
                        Debug.LogError("Sprite reference is missing or index is out of range!");
                    }
                    Debug.Log(action.speaker.sprites[0] == null);
                    controller.Setup(action.speaker.sprites[action.spriteIndex]);
                    controller.Show(action.coords);
                }
                catch (UnassignedReferenceException)
                {
                    Debug.LogError($"Failed to instantiate sprite for speaker: {action.speaker.speakerName}. Make sure the prefab is assigned.");
                }

                return;

            case StoryScene.Sentence.Action.Type.MOVE:
                if (sprites.ContainsKey(action.speaker))
                {
                    controller = sprites[action.speaker];
                    controller.Move(action.coords, action.moveSpeed);
                }
                else
                {
                    Debug.LogWarning($"Attempted to move non-existent sprite for speaker: {action.speaker.speakerName}");
                }
                return;

            case StoryScene.Sentence.Action.Type.DISAPPEAR:
                if (sprites.ContainsKey(action.speaker))
                {
                    controller = sprites[action.speaker];
                    controller.Hide();
                    sprites.Remove(action.speaker);
                    Destroy(controller.gameObject);
                }
                else
                {
                    Debug.LogWarning($"Attempted to hide non-existent sprite for speaker: {action.speaker.speakerName}");
                }
                return;

            case StoryScene.Sentence.Action.Type.NONE:
                if (sprites.ContainsKey(action.speaker))
                {
                    controller = sprites[action.speaker];
                    controller.SwitchSprite(action.speaker.sprites[action.spriteIndex]);
                }
                else
                {
                    Debug.LogWarning($"Attempted to switch sprite for non-existent speaker: {action.speaker.speakerName}");
                }
                return;

                 case StoryScene.Sentence.Action.Type.BOUNCE:
                if (sprites.ContainsKey(action.speaker))
                {
                    controller = sprites[action.speaker];
                    controller.Bounce();
                }
                else
                {
                    Debug.LogWarning($"it didnt work bruh");
                }
                return;
        }
    }
}


