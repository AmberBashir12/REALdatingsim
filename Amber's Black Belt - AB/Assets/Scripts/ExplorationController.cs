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
    
    private ExplorationScene currentScene;
    private GameController gameController;
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    private string fullDialogueText;
    private bool isTyping = false;

    
    void Start()
    {
        gameController = FindObjectOfType<GameController>();

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
        currentScene = scene;
        
        // Set background through GameController's background system
        if (scene.background != null && gameController != null)
        {
            if (gameController.backgroundController != null)
            {
                gameController.backgroundController.SwitchImage(scene.background);
            }
            else
            {
                Debug.LogWarning("GameController.backgroundController is null!");
            }
        }
        else
        {
            if (scene.background == null)
                Debug.LogWarning("ExplorationScene has no background assigned!");
            if (gameController == null)
                Debug.LogWarning("GameController reference is null!");
        }
        
        // Clear existing elements
        ClearScene();
        
        // Setup speakers
        SetupSpeakers();
        
        // Setup interactive objects
        SetupInteractiveObjects();
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
            return;
        }
        if (speakerContainer == null)
        {
            Debug.LogWarning("Speaker container is null!");
            return;
        }
        
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
            fullDialogueText = TextTemplate.Resolve(text);
            
            // Set speaker name and color if provided
            if (speaker != null && speakerNameText != null)
            {
                speakerNameText.text = TextTemplate.Resolve(speaker.speakerName);
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
        // Ensure dialogue panel is closed and reset
        CloseDialogue();
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        if (speakerNameText != null)
        {
            speakerNameText.text = "";
        }
        fullDialogueText = "";
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
            CloseDialogue();

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            ClearScene();
            gameController.PlayScene(nextScene, -1, true);
        }
        else
        {
            Debug.LogWarning("GameController is null in NavigateToScene!");
        }
    }
}