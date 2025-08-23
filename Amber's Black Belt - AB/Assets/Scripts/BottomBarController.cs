using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using System;
using NarrativeRuntime;
using Narrative;

public class BottomBarController : MonoBehaviour
{
    public TextMeshProUGUI barText;
    public TextMeshProUGUI personNameText;

    private int sentenceIndex = -1;
    public StoryScene currentScene;
    // Runtime mode fields
    private RuntimeStoryScene runtimeScene;
    private List<Narrative.SentenceData> runtimeSentences;
    private bool isRuntime = false;
    private State state = State.COMPLETED;
    private Animator animator;
    public bool IsHidden = false;

    public Dictionary<Speaker, SpriteController> sprites = new Dictionary<Speaker, SpriteController>();
    // Runtime sprite controllers keyed by speaker id
    private Dictionary<string, SpriteController> runtimeSprites = new Dictionary<string, SpriteController>();

    // public Speaker[] Speakers;
    // public SpriteController[] SpriteControllers;
    public GameObject spritesPrefab;

    private enum State
    {
        PLAYING, COMPLETED
    }

    private void Awake()
    {
        // Ensure animator reference (some prefabs might be missing it)
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
            if (!animator)
            {
                Debug.LogWarning("BottomBarController has no Animator component; Hide/Show/Bounce animations will be skipped.");
            }
        }
    }

    public void Hide()
    {
        if (IsHidden) return;
        if (animator)
        {
            animator.SetTrigger("Hide");
        }
        else
        {
            // Fallback: simply deactivate text objects visually if desired
            // (leave active so logic still progresses)
        }
        IsHidden = true;
    }

    public void Show()
    {
        if (!IsHidden) return;
        if (animator)
        {
            animator.SetTrigger("Show");
        }
        IsHidden = false;
    }

    public void Bounce()
    {
        if (animator)
        {
            animator.SetTrigger("Bounce");
        }
    }

    public void ClearText()
    {
        barText.text = "";
    }

    public void PlayScene(StoryScene scene)
    {
    isRuntime = false;
    runtimeScene = null;
    runtimeSentences = null;
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

    // Runtime narrative support (temporary adapter methods)
    public void PlayRuntimeStory(NarrativeRuntime.RuntimeStoryScene runtimeScene)
    {
        // Full runtime path (no ScriptableObject bridge)
        isRuntime = true;
        currentScene = null;
        this.runtimeScene = runtimeScene;
        runtimeSentences = runtimeScene.Sentences;
        sentenceIndex = -1;
        // Clear any prior runtime sprites
        foreach (var kv in runtimeSprites)
        {
            if (kv.Value) Destroy(kv.Value.gameObject);
        }
        runtimeSprites.Clear();
        PlayNextSentence();
    }

    public void PlayNextSentence()
    {
        if (isRuntime)
        {
            PlayNextRuntimeSentence();
            return;
        }

        // Original SO path
        if (currentScene == null || currentScene.sentences == null || currentScene.sentences.Count == 0)
        {
            Debug.LogError("PlayNextSentence called, but currentScene is null or has no sentences.");
            state = State.COMPLETED;
            return;
        }
        if (sentenceIndex + 1 >= currentScene.sentences.Count)
        {
            Debug.LogWarning("PlayNextSentence called, but already at/past the last sentence.");
            state = State.COMPLETED;
            return;
        }
        sentenceIndex++;
        StartCoroutine(TypeText(currentScene.sentences[sentenceIndex].text));
        Speaker speaker = currentScene.sentences[sentenceIndex].speaker;
        if (speaker != null)
        {
            personNameText.text = speaker.speakerName;
            personNameText.color = speaker.textColor;
        }
        else
        {
            personNameText.text = "";
        }
        ActSpeakers();
    }

    private void PlayNextRuntimeSentence()
    {
        if (runtimeSentences == null || runtimeSentences.Count == 0)
        {
            state = State.COMPLETED;
            return;
        }
        if (sentenceIndex + 1 >= runtimeSentences.Count)
        {
            state = State.COMPLETED;
            return;
        }
        sentenceIndex++;
        var sentence = runtimeSentences[sentenceIndex];
        StartCoroutine(TypeText(sentence.text));
        if (!string.IsNullOrEmpty(sentence.speaker) && NarrativeRegistry.Speakers.TryGetValue(sentence.speaker, out var sp))
        {
            personNameText.text = sp.Name;
            personNameText.color = sp.Color;
        }
        else
        {
            personNameText.text = "";
        }
        ActRuntimeSpeakers(sentence);
    }

    public bool IsCompleted()
    {
        return state == State.COMPLETED;
    }

    public bool IsLastSentence()
    {
        if (isRuntime)
        {
            if (runtimeSentences == null) return true;
            return sentenceIndex + 1 >= runtimeSentences.Count;
        }
        if (currentScene == null || currentScene.sentences == null) return true;
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

    private void ActRuntimeSpeakers(Narrative.SentenceData sentence)
    {
        if (sentence.actions == null) return;
        foreach (var action in sentence.actions)
        {
            RuntimeActSpeaker(action);
        }
    }

    private void RuntimeActSpeaker(Narrative.ActionData action)
    {
        if (string.IsNullOrEmpty(action.speaker)) return;
        if (!NarrativeRegistry.Speakers.TryGetValue(action.speaker, out var speakerRuntime))
        {
            Debug.LogWarning($"Runtime speaker not found: {action.speaker}");
            return;
        }
        runtimeSprites.TryGetValue(action.speaker, out var controller);
        var pos = new Vector2(action.x, action.y);
        switch (action.type)
        {
            case ActionType.APPEAR:
                if (!controller)
                {
                    if (!speakerRuntime.Prefab)
                    {
                        Debug.LogWarning($"Speaker prefab missing for {speakerRuntime.Id}");
                        return;
                    }
                    controller = Instantiate(speakerRuntime.Prefab.gameObject, spritesPrefab.transform).GetComponent<SpriteController>();
                    runtimeSprites[action.speaker] = controller;
                }
                if (speakerRuntime.Sprites.Count <= action.spriteIndex || action.spriteIndex < 0)
                {
                    Debug.LogWarning($"Sprite index {action.spriteIndex} out of range for speaker {speakerRuntime.Id}");
                    return;
                }
                controller.Setup(speakerRuntime.Sprites[action.spriteIndex]);
                controller.Show(pos);
                break;
            case ActionType.MOVE:
                if (controller)
                {
                    controller.Move(pos, action.moveSpeed <= 0 ? 1f : action.moveSpeed);
                }
                break;
            case ActionType.DISAPPEAR:
                if (controller)
                {
                    controller.Hide();
                    runtimeSprites.Remove(action.speaker);
                    Destroy(controller.gameObject);
                }
                break;
            case ActionType.NONE:
                if (controller)
                {
                    if (speakerRuntime.Sprites.Count > action.spriteIndex && action.spriteIndex >= 0)
                        controller.SwitchSprite(speakerRuntime.Sprites[action.spriteIndex]);
                }
                break;
            case ActionType.BOUNCE:
                if (controller)
                {
                    controller.Bounce();
                }
                break;
        }
    }
}


