using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExplorationController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Button dialogueCloseButton;
    
    [Header("Prefab Containers")]
    public RectTransform speakerContainer;
    public RectTransform objectContainer;
    public Canvas mainCanvas;
    
    private ExplorationScene currentScene;
    private GameController gameController;
    
    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    
    private Coroutine typingCoroutine;
    private string fullDialogueText;
    private bool isTyping = false;
    
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        
        // Find main canvas if not assigned
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
        
        // Setup dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (dialogueCloseButton != null)
        {
            dialogueCloseButton.onClick.AddListener(CloseDialogue);
        }
        
        // Set up click detection for dialogue panel to complete/close text
        if (dialoguePanel != null)
        {
            // Add Button component to dialogue panel if it doesn't exist
            Button dialoguePanelButton = dialoguePanel.GetComponent<Button>();
            if (dialoguePanelButton == null)
            {
                dialoguePanelButton = dialoguePanel.AddComponent<Button>();
            }
            dialoguePanelButton.onClick.AddListener(OnDialogueClicked);
        }
    }
    
    public void SetupExplorationScene(ExplorationScene scene)
    {
        Debug.Log($"SetupExplorationScene called for: {scene.name}");
        currentScene = scene;
        
        // Set background through GameController's background system
        Debug.Log("Attempting to set background...");
        if (scene.background != null && gameController != null)
        {
            Debug.Log($"Setting exploration scene background: {scene.background.name}");
            if (gameController.backgroundController != null)
            {
                Debug.Log($"BackgroundController found, calling SwitchImage with sprite: {scene.background.name}");
                gameController.backgroundController.SwitchImage(scene.background);
                Debug.Log("Background set through GameController.backgroundController.SwitchImage()");
            }
            else
            {
                Debug.LogError("GameController.backgroundController is null!");
            }
        }
        else
        {
            if (scene.background == null)
                Debug.LogError("ExplorationScene has no background assigned!");
            if (gameController == null)
                Debug.LogError("GameController reference is null!");
        }
        
        // Clear existing elements
        ClearScene();
        
        // Setup speakers
        SetupSpeakers();
        
        // Setup interactive objects
        SetupInteractiveObjects();
        
        Debug.Log($"Exploration scene setup complete: {scene.name}");
    }
    
    private void ClearScene()
    {
        // Clear speakers
        if (speakerContainer != null)
        {
            foreach (Transform child in speakerContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Clear objects
        if (objectContainer != null)
        {
            foreach (Transform child in objectContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    private void SetupSpeakers()
    {
        if (currentScene.speakers == null)
        {
            Debug.Log("No speakers in current scene");
            return;
        }
        if (speakerContainer == null)
        {
            Debug.LogWarning("Speaker container is null!");
            return;
        }
        
        Debug.Log($"Setting up {currentScene.speakers.Count} speakers");
        
        foreach (var speakerData in currentScene.speakers)
        {
            // Instantiate speaker from prefab
            GameObject speakerObj = Instantiate(speakerData.speaker.prefab.gameObject, speakerContainer);
            speakerObj.name = $"Speaker_{speakerData.speaker.speakerName}";
            
            // Position the speaker using canvas coordinates
            RectTransform rectTransform = speakerObj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = speakerObj.AddComponent<RectTransform>();
            }
            
            rectTransform.anchoredPosition = speakerData.coords;
            
            // Setup sprite controller if it exists
            SpriteController spriteController = speakerObj.GetComponent<SpriteController>();
            if (spriteController != null && speakerData.speaker.sprites != null && speakerData.speaker.sprites.Count > 0)
            {
                spriteController.Setup(speakerData.speaker.sprites[0]); // Use first sprite as default
            }
            
            // Add interactive speaker controller
            InteractiveSpeakerController speakerInteractionController = speakerObj.GetComponent<InteractiveSpeakerController>();
            if (speakerInteractionController == null)
            {
                speakerInteractionController = speakerObj.AddComponent<InteractiveSpeakerController>();
            }
            speakerInteractionController.Setup(speakerData.dialogueText, this, speakerData.speaker);
            
            Debug.Log($"Created speaker: {speakerData.speaker.speakerName} at position {speakerData.coords}");
        }
    }
    
    private void SetupInteractiveObjects()
    {
        if (currentScene.interactableObjects == null || objectContainer == null) return;
        
        for (int i = 0; i < currentScene.interactableObjects.Count; i++)
        {
            var objectData = currentScene.interactableObjects[i];
            
            if (objectData.objectPrefab == null) continue;
            
            // Instantiate the prefab
            GameObject obj = Instantiate(objectData.objectPrefab, objectContainer);
            obj.name = $"InteractiveObject_{i}";
            
            // Position the object using canvas coordinates
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = obj.AddComponent<RectTransform>();
            }
            
            rectTransform.anchoredPosition = objectData.coords;
            
            // Add or setup interactive controller
            InteractiveObjectController controller = obj.GetComponent<InteractiveObjectController>();
            if (controller == null)
            {
                controller = obj.AddComponent<InteractiveObjectController>();
            }
            
            controller.Setup(objectData.nextScene, objectData.soundOnClick, 
                           objectData.requiredChoiceKey, objectData.choiceKeyToUnlock);
        }
    }
    
    public void ShowDialogue(string text, Speaker speaker = null)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            // Store the full text
            fullDialogueText = text;
            
            // Set speaker name and color if provided
            if (speaker != null && speakerNameText != null)
            {
                speakerNameText.text = speaker.speakerName;
                speakerNameText.color = speaker.textColor;
            }
            else if (speakerNameText != null)
            {
                speakerNameText.text = ""; // Clear name if no speaker
            }
            
            dialoguePanel.SetActive(true);
            
            // Start typing animation
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(fullDialogueText));
        }
    }
    
    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            
            // Stop typing if still in progress
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;
        }
    }
    
    public void ResetDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
        fullDialogueText = "";
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text = text.Substring(0, i + 1);
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        typingCoroutine = null;
    }
    
    public void OnDialogueClicked()
    {
        if (isTyping)
        {
            // Complete the text immediately
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            dialogueText.text = fullDialogueText;
            isTyping = false;
        }
        else
        {
            // Close dialogue if text is complete
            CloseDialogue();
        }
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && gameController != null && gameController.audioController != null)
        {
            // Use GameController's audio system for sound effects
            gameController.audioController.PlayAudio(null, clip);
        }
    }
    
    public void NavigateToScene(GameScene nextScene)
    {
        if (gameController != null)
        {
            // Clear current exploration scene elements before transitioning
            Debug.Log($"Navigating from exploration scene to: {nextScene.name}");
            Debug.Log("Clearing exploration scene elements before transitioning to next scene");
            ClearScene();
            
            // Reset dialogue panel visibility
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            gameController.PlayScene(nextScene);
        }
        else
        {
            Debug.LogError("GameController is null in NavigateToScene!");
        }
    }
}