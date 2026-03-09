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

    private Coroutine currentTextCoroutine;
    private string currentFullText = "";
    private GameController gameController;
    [SerializeField] private float inputCooldownSeconds = 0.08f;
    private float lastAdvanceInputTime = -999f;
    private bool waitingForSentenceChoice = false;
    private List<StoryScene.FollowUpSentence> activeBranchSentences;
    private int branchSentenceIndex = -1;
    private List<int> visibleSentenceChoiceOptionIndices = new List<int>();
    private bool waitingForBranchChoice = false;
    private List<StoryScene.FollowUpLine> activeBranchChoiceLines;
    private int branchChoiceLineIndex = -1;
    private List<int> visibleBranchChoiceOptionIndices = new List<int>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && gameController.GetCurrentState() == GameController.State.IDLE) // Left mouse click or spacebar, only when game is idle
        {
            if (Time.unscaledTime - lastAdvanceInputTime < inputCooldownSeconds)
            {
                return;
            }

            lastAdvanceInputTime = Time.unscaledTime;
            AdvanceSentence();
        }
    }

    private void AdvanceSentence()
    {
        if (state == State.PLAYING)
        {
            // Skip to the end of current text
            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
            }
            barText.text = currentFullText;
            state = State.COMPLETED;
        }
        else if (state == State.COMPLETED)
        {
            if (waitingForSentenceChoice || waitingForBranchChoice)
            {
                return;
            }

            if (IsBlockingSoundPlaying())
            {
                return;
            }

            if (activeBranchChoiceLines != null)
            {
                if (branchChoiceLineIndex + 1 < activeBranchChoiceLines.Count)
                {
                    branchChoiceLineIndex++;
                    ShowFollowUpLine(activeBranchChoiceLines[branchChoiceLineIndex]);
                    return;
                }

                activeBranchChoiceLines = null;
                branchChoiceLineIndex = -1;
            }

            if (activeBranchSentences != null)
            {
                if (branchSentenceIndex + 1 < activeBranchSentences.Count)
                {
                    branchSentenceIndex++;
                    StoryScene.FollowUpSentence followUpSentence = activeBranchSentences[branchSentenceIndex];
                    if (followUpSentence.followUpType == StoryScene.FollowUpSentence.FollowUpType.CHOICE)
                    {
                        ShowBranchChoice(followUpSentence);
                        return;
                    }

                    ShowFollowUpSentence(followUpSentence);
                    return;
                }

                activeBranchSentences = null;
                branchSentenceIndex = -1;
            }

            // Move to next sentence
            if (!IsLastSentence())
            {
                PlayNextSentence();
            }
            else
            {
                // We're at the last sentence, move to the next scene
                if (gameController != null && currentScene != null)
                {
                    gameController.PlayScene(currentScene.GetNextScene());
                }
            }
        }
    }

    private bool IsBlockingSoundPlaying()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
        }

        return gameController != null
            && gameController.audioController != null
            && gameController.audioController.IsSoundPlaying();
    }

    public int GetSentenceIndex()
    {
        return sentenceIndex;
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

    private void ClearSprites()
    {
        // Destroy all sprite GameObjects and clear the dictionary
        foreach (var controller in sprites.Values)
        {
            if (controller != null && controller.gameObject != null)
            {
                Destroy(controller.gameObject);
            }
        }
        sprites.Clear();
    }

    public void PlayScene(StoryScene scene)
    {
        PlayScene(scene, -1);
    }

    public void PlayScene(StoryScene scene, int startSentenceIndex)
    {
        // Clear previous speakers before starting a new scene
        ClearSprites();
        
        currentScene = scene;
        sentenceIndex = startSentenceIndex - 1;
        waitingForSentenceChoice = false;
        activeBranchSentences = null;
        branchSentenceIndex = -1;
        waitingForBranchChoice = false;
        activeBranchChoiceLines = null;
        branchChoiceLineIndex = -1;
        visibleSentenceChoiceOptionIndices.Clear();
        visibleBranchChoiceOptionIndices.Clear();

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
            if (startSentenceIndex < 0 || startSentenceIndex >= currentScene.sentences.Count)
            {
                sentenceIndex = -1;
            }

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

        StoryScene.Sentence sentence = currentScene.sentences[sentenceIndex];
        if (sentence.sentenceType == StoryScene.Sentence.SentenceType.CHOICE)
        {
            ShowSentenceChoice(sentence);
            return;
        }

        ShowSentence(sentence);
    }

    private void ShowSentenceChoice(StoryScene.Sentence sentence)
    {
        List<StoryScene.Sentence.ChoiceOption> options = sentence.choice.options;
        if (options == null || options.Count == 0)
        {
            if (!IsLastSentence())
            {
                PlayNextSentence();
            }
            else if (gameController != null && currentScene != null)
            {
                gameController.PlayScene(currentScene.GetNextScene());
            }
            return;
        }

        barText.text = TextTemplate.Resolve(sentence.choice.prompt);
        personNameText.text = "";
        state = State.COMPLETED;

        List<string> optionTexts = new List<string>();
        visibleSentenceChoiceOptionIndices.Clear();
        for (int i = 0; i < options.Count; i++)
        {
            StoryScene.Sentence.ChoiceOption option = options[i];
            if (!AreRequiredChoiceKeysUnlocked(option.requiredChoiceKeys))
            {
                continue;
            }

            optionTexts.Add(TextTemplate.Resolve(option.text));
            visibleSentenceChoiceOptionIndices.Add(i);
        }

        if (optionTexts.Count == 0)
        {
            Debug.LogWarning($"StoryScene '{currentScene?.name}' sentence {sentenceIndex} has no available choice options (all locked or empty).");

            if (!IsLastSentence())
            {
                PlayNextSentence();
            }
            else if (gameController != null && currentScene != null)
            {
                gameController.PlayScene(currentScene.GetNextScene());
            }
            return;
        }

        waitingForSentenceChoice = true;

        if (gameController != null)
        {
            gameController.SetChooseState(true);
        }

        if (gameController != null && gameController.chooseController != null)
        {
            gameController.chooseController.SetupInlineChoose(optionTexts, OnSentenceChoiceSelected);
        }
        else
        {
            waitingForSentenceChoice = false;
            if (gameController != null)
            {
                gameController.SetChooseState(false);
            }
            Debug.LogError("ChooseController reference is missing on GameController. Cannot show sentence choice.");
        }
    }

    private void OnSentenceChoiceSelected(int optionIndex)
    {
        waitingForSentenceChoice = false;

        if (gameController != null)
        {
            gameController.SetChooseState(false);
        }

        if (currentScene == null || currentScene.sentences == null || sentenceIndex < 0 || sentenceIndex >= currentScene.sentences.Count)
        {
            return;
        }

        StoryScene.Sentence choiceSentence = currentScene.sentences[sentenceIndex];
        List<StoryScene.Sentence.ChoiceOption> options = choiceSentence.choice.options;
        if (options == null || optionIndex < 0 || optionIndex >= visibleSentenceChoiceOptionIndices.Count)
        {
            return;
        }

        int sourceOptionIndex = visibleSentenceChoiceOptionIndices[optionIndex];
        if (sourceOptionIndex < 0 || sourceOptionIndex >= options.Count)
        {
            return;
        }

        StoryScene.Sentence.ChoiceOption selectedOption = options[sourceOptionIndex];

        if (!string.IsNullOrEmpty(selectedOption.choiceKeyToUnlock) && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockChoice(selectedOption.choiceKeyToUnlock);
        }

        if (selectedOption.followUpSentences != null && selectedOption.followUpSentences.Count > 0)
        {
            activeBranchSentences = selectedOption.followUpSentences;
            branchSentenceIndex = -1;
            AdvanceSentence();
            return;
        }

        if (!IsLastSentence())
        {
            PlayNextSentence();
        }
        else if (gameController != null && currentScene != null)
        {
            gameController.PlayScene(currentScene.GetNextScene());
        }
    }

    private bool AreRequiredChoiceKeysUnlocked(List<string> requiredChoiceKeys)
    {
        if (requiredChoiceKeys == null || requiredChoiceKeys.Count == 0)
        {
            return true;
        }

        if (GameStateManager.Instance == null)
        {
            return false;
        }

        for (int i = 0; i < requiredChoiceKeys.Count; i++)
        {
            string requiredKey = requiredChoiceKeys[i];
            if (string.IsNullOrEmpty(requiredKey))
            {
                continue;
            }

            if (!GameStateManager.Instance.IsChoiceUnlocked(requiredKey))
            {
                return false;
            }
        }

        return true;
    }

    private void ShowBranchChoice(StoryScene.FollowUpSentence followUpSentence)
    {
        List<StoryScene.FollowUpChoiceOption> options = followUpSentence.choice.options;
        if (options == null || options.Count == 0)
        {
            AdvanceSentence();
            return;
        }

        barText.text = TextTemplate.Resolve(followUpSentence.choice.prompt);
        personNameText.text = "";
        state = State.COMPLETED;

        List<string> optionTexts = new List<string>();
        visibleBranchChoiceOptionIndices.Clear();
        for (int i = 0; i < options.Count; i++)
        {
            StoryScene.FollowUpChoiceOption option = options[i];
            if (!AreRequiredChoiceKeysUnlocked(option.requiredChoiceKeys))
            {
                continue;
            }

            optionTexts.Add(TextTemplate.Resolve(option.text));
            visibleBranchChoiceOptionIndices.Add(i);
        }

        if (optionTexts.Count == 0)
        {
            AdvanceSentence();
            return;
        }

        waitingForBranchChoice = true;

        if (gameController != null)
        {
            gameController.SetChooseState(true);
        }

        if (gameController != null && gameController.chooseController != null)
        {
            gameController.chooseController.SetupInlineChoose(optionTexts, OnBranchChoiceSelected);
        }
        else
        {
            waitingForBranchChoice = false;
            if (gameController != null)
            {
                gameController.SetChooseState(false);
            }
            Debug.LogError("ChooseController reference is missing on GameController. Cannot show branch choice.");
        }
    }

    private void OnBranchChoiceSelected(int optionIndex)
    {
        waitingForBranchChoice = false;

        if (gameController != null)
        {
            gameController.SetChooseState(false);
        }

        if (activeBranchSentences == null || branchSentenceIndex < 0 || branchSentenceIndex >= activeBranchSentences.Count)
        {
            return;
        }

        StoryScene.FollowUpSentence choiceSentence = activeBranchSentences[branchSentenceIndex];
        List<StoryScene.FollowUpChoiceOption> options = choiceSentence.choice.options;
        if (options == null || optionIndex < 0 || optionIndex >= visibleBranchChoiceOptionIndices.Count)
        {
            return;
        }

        int sourceOptionIndex = visibleBranchChoiceOptionIndices[optionIndex];
        if (sourceOptionIndex < 0 || sourceOptionIndex >= options.Count)
        {
            return;
        }

        StoryScene.FollowUpChoiceOption selectedOption = options[sourceOptionIndex];

        if (!string.IsNullOrEmpty(selectedOption.choiceKeyToUnlock) && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockChoice(selectedOption.choiceKeyToUnlock);
        }

        if (selectedOption.followUpLines != null && selectedOption.followUpLines.Count > 0)
        {
            activeBranchChoiceLines = selectedOption.followUpLines;
            branchChoiceLineIndex = 0;
            ShowFollowUpLine(activeBranchChoiceLines[branchChoiceLineIndex]);
            return;
        }

        AdvanceSentence();
    }

    private void ShowSentence(StoryScene.Sentence sentence)
    {
        ShowDialogueSentence(sentence.speaker, sentence.text, sentence.actions, sentence.music, sentence.sound);
    }

    private void ShowFollowUpSentence(StoryScene.FollowUpSentence sentence)
    {
        ShowDialogueSentence(sentence.speaker, sentence.text, sentence.actions, sentence.music, sentence.sound);
    }

    private void ShowFollowUpLine(StoryScene.FollowUpLine line)
    {
        ShowDialogueSentence(line.speaker, line.text, line.actions, line.music, line.sound);
    }

    private void ShowDialogueSentence(Speaker speaker, string text, List<StoryScene.Sentence.Action> actions, AudioClip music, AudioClip sound)
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
        }

        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
        }

        string resolvedText = TextTemplate.Resolve(text);
        currentTextCoroutine = StartCoroutine(TypeText(resolvedText));

        if (speaker != null)
        {
            personNameText.text = TextTemplate.Resolve(speaker.speakerName);
            personNameText.color = speaker.textColor;
        }
        else
        {
            personNameText.text = "";
        }

        ActSpeakers(actions);

        if (gameController != null && gameController.audioController != null)
        {
            gameController.audioController.PlayAudio(music, sound);
        }
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
        currentFullText = text;
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

    private void ActSpeakers(List<StoryScene.Sentence.Action> actions)
    {
        if (actions == null)
        {
            return;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            Debug.Log(i);
            ActSpeaker(actions[i]);
        }
    }

    private void ActSpeaker(StoryScene.Sentence.Action action)
    {
        SpriteController controller = null;
        float tintStrength = action.tintOpacity;
        if (tintStrength <= 0f && action.tintColor.a > 0f)
        {
            tintStrength = action.tintColor.a;
        }

        Color tintTarget = new Color(action.tintColor.r, action.tintColor.g, action.tintColor.b, 1f);
        Color resolvedTint = Color.Lerp(Color.white, tintTarget, Mathf.Clamp01(tintStrength));
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
                    controller.SetTint(resolvedTint);
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
                    controller.SetTint(resolvedTint);
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


