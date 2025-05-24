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

    public void ClearText()
    {
        barText.text = "";
    }

    public void PlayScene(StoryScene scene)
    {
        currentScene = scene;
        sentenceIndex = -1;
        PlayNextSentence();
    }

    public void PlayNextSentence()
    {
        StartCoroutine(TypeText(currentScene.sentences[++sentenceIndex].text));
        personNameText.text = currentScene.sentences[sentenceIndex].speaker.speakerName;
        personNameText.color = currentScene.sentences[sentenceIndex].speaker.textColor;
        ActSpeakers();
    }

    public bool IsCompleted()
    {
        return state == State.COMPLETED;
    }

    public bool IsLastSentence()
    {
        return sentenceIndex + 1 == currentScene.sentences.Count;
    }

    private IEnumerator TypeText(string text)
    {
        barText.text = "";
        state = State.PLAYING;
        int wordIndex = 0;

        while (wordIndex < text.Length)
        {
            barText.text += text[wordIndex];
            yield return new WaitForSeconds(0.05f);
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
                    controller.Setup(action.speaker.sprites[0]);
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
        }
    }
}


