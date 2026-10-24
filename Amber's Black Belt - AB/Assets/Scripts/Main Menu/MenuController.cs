using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MenuController : MonoBehaviour
{
    public string gameScene;
    public AudioClip clickSound;
    public CanvasGroup fadePanel;
    public float fadeDuration = 3f;
    public TMP_InputField nameInputField;

    [Header("New Game Flow")]
    public GameObject selectionPanel;
    public GameObject namePanel;

    [Header("Name Settings")]
    [Min(1)]
    public int nameCharacterLimit = 16;

    [Tooltip("Case-insensitive. Add words/substrings you want to block.")]
    public string[] bannedWords;

    private string lastValidName = "";
    private bool suppressNameCallback;

    private enum NewGameFlowState
    {
        Idle,
        ChoosingCharacter,
        EnteringName
    }

    private NewGameFlowState flowState = NewGameFlowState.Idle;
    private bool isStartingGame;

    private void Start()
    {
        // Make sure these start hidden.
        if (selectionPanel == null)
        {
            selectionPanel = GameObject.Find("SelectionPanel");
        }
        if (namePanel == null)
        {
            // In this scene the name panel root object is currently named "Image".
            namePanel = GameObject.Find("Image");
        }

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
        if (namePanel != null)
        {
            namePanel.SetActive(false);
        }

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
        }

        if (nameInputField != null)
        {
            nameInputField.characterLimit = nameCharacterLimit;

            // Initialize field from saved value (sanitized).
            string initial = NameModeration.Sanitize(PlayerProfile.Name, nameCharacterLimit);
            lastValidName = initial;

            suppressNameCallback = true;
            nameInputField.text = initial;
            suppressNameCallback = false;

            // Auto-wire so you don't have to hook events in the Inspector.
            nameInputField.onValueChanged.AddListener(OnNameInputChanged);
            nameInputField.onEndEdit.AddListener(OnNameInputSubmitted);
            nameInputField.onSubmit.AddListener(OnNameInputSubmitted);
        }
    }

    private void ShowCharacterSelection()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
        }
        if (namePanel != null)
        {
            namePanel.SetActive(false);
        }

        flowState = NewGameFlowState.ChoosingCharacter;
    }

    private void ShowNameEntry()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
        if (namePanel != null)
        {
            namePanel.SetActive(true);
        }

        flowState = NewGameFlowState.EnteringName;

        if (nameInputField != null)
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    public void ChooseFemale()
    {
        PlayerProfile.SetCharacterChoice("Female");
        ShowNameEntry();
    }

    public void ChooseMale()
    {
        PlayerProfile.SetCharacterChoice("Male");
        ShowNameEntry();
    }

    public void ChooseOther()
    {
        PlayerProfile.SetCharacterChoice("Other");
        ShowNameEntry();
    }

    // Optional: hook this up to the TMP_InputField OnValueChanged/OnEndEdit event.
    public void OnNameInputChanged(string value)
    {
        if (suppressNameCallback)
        {
            return;
        }

        if (nameInputField == null)
        {
            return;
        }

        // Sanitize first (also enforces limit in code).
        string sanitized = NameModeration.Sanitize(value, nameCharacterLimit);

        if (sanitized != value)
        {
            suppressNameCallback = true;
            nameInputField.text = sanitized;
            suppressNameCallback = false;
        }

        if (NameModeration.IsAllowed(sanitized, bannedWords, out string reason))
        {
            lastValidName = sanitized;
            PlayerProfile.SetName(sanitized);
            SetPlaceholderMessage("Enter text...");
        }
        else
        {
            // Revert to last known good name.
            suppressNameCallback = true;
            nameInputField.text = lastValidName;
            suppressNameCallback = false;

            SetPlaceholderMessage(reason == null ? "Invalid name" : "Invalid name");
        }
    }

    // Called when the player presses Enter/Done or otherwise ends editing.
    private void OnNameInputSubmitted(string value)
    {
        if (isStartingGame)
        {
            return;
        }

        if (flowState != NewGameFlowState.EnteringName)
        {
            return;
        }

        if (nameInputField == null)
        {
            return;
        }

        // Run the same sanitize + moderation flow.
        string sanitized = NameModeration.Sanitize(value, nameCharacterLimit);
        if (sanitized != value)
        {
            suppressNameCallback = true;
            nameInputField.text = sanitized;
            suppressNameCallback = false;
        }

        if (!NameModeration.IsAllowed(sanitized, bannedWords, out _))
        {
            suppressNameCallback = true;
            nameInputField.text = lastValidName;
            suppressNameCallback = false;
            return;
        }

        lastValidName = sanitized;
        PlayerProfile.SetName(sanitized);

        StartGame();
    }

    private void StartGame()
    {
        if (isStartingGame)
        {
            return;
        }

        isStartingGame = true;
        StartCoroutine(FadeToBlackAndLoad());
    }

    private bool ValidateNameForStart()
    {
        if (nameInputField == null)
        {
            return true;
        }

        string sanitized = NameModeration.Sanitize(nameInputField.text, nameCharacterLimit);
        if (!NameModeration.IsAllowed(sanitized, bannedWords, out _))
        {
            suppressNameCallback = true;
            nameInputField.text = lastValidName;
            suppressNameCallback = false;
            return false;
        }

        lastValidName = sanitized;
        PlayerProfile.SetName(sanitized);
        return true;
    }

    private void SetPlaceholderMessage(string message)
    {
        if (nameInputField == null)
        {
            return;
        }

        if (nameInputField.placeholder is TMP_Text tmpText)
        {
            tmpText.text = message;
        }
    }

    public void NewGame()
    {
        PlayClickSound();

        // Flow:
        // 1) Click New Game => show character selection
        // 2) Click Female/Male/Other => show name panel
        // 3) Click New Game again => validate name and load
        if (flowState == NewGameFlowState.Idle)
        {
            ShowCharacterSelection();
            return;
        }

        if (flowState == NewGameFlowState.ChoosingCharacter)
        {
            // Wait until the player chooses a character.
            return;
        }

        if (!ValidateNameForStart())
        {
            // Block starting if the name is invalid.
            return;
        }

        StartGame();
    }

    private IEnumerator FadeToBlackAndLoad()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }
        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
        }
    }
}

