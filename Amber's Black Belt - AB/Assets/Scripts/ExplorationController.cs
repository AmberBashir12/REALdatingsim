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
    public Button dialogueCloseButton;
    
    [Header("Prefab Containers")]
    public RectTransform speakerContainer;
    public RectTransform objectContainer;
    public Canvas mainCanvas;
    
    private ExplorationScene currentScene;
    private GameController gameController;
    
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
    }
    
    public void SetupExplorationScene(ExplorationScene scene)
    {
        currentScene = scene;
        
        // Set background through GameController's background system
        if (scene.background != null && gameController != null)
        {
            gameController.backgroundController.SetImage(scene.background);
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
        if (currentScene.speakers == null || speakerContainer == null) return;
        
        foreach (var speakerData in currentScene.speakers)
        {
            // Create speaker object (you'll need a speaker prefab)
            GameObject speakerObj = new GameObject($"Speaker_{speakerData.speaker.speakerName}");
            speakerObj.transform.SetParent(speakerContainer);
            
            // Position the speaker using screen-relative coordinates
            RectTransform rectTransform = speakerObj.AddComponent<RectTransform>();
            Vector2 screenPos = ConvertScreenPositionToCanvasPosition(speakerData.screenPosition);
            rectTransform.anchoredPosition = screenPos;
            rectTransform.localScale = Vector3.one * speakerData.scale;
            
            // Add sprite renderer for speaker image
            if (speakerData.speaker.sprites != null && speakerData.speaker.sprites.Count > 0 && speakerData.speaker.sprites[0] != null)
            {
                SpriteRenderer spriteRenderer = speakerObj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = speakerData.speaker.sprites[0]; // Use first sprite as default
                spriteRenderer.sortingOrder = 1;
            }
            
            // Add interactive speaker controller
            InteractiveSpeakerController speakerController = speakerObj.AddComponent<InteractiveSpeakerController>();
            speakerController.Setup(speakerData.dialogueText, this);
            
            // Add collider for interaction
            BoxCollider2D collider = speakerObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
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
            
            // Position the object using screen-relative coordinates
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = obj.AddComponent<RectTransform>();
            }
            
            Vector2 screenPos = ConvertScreenPositionToCanvasPosition(objectData.screenPosition);
            rectTransform.anchoredPosition = screenPos;
            rectTransform.localScale = Vector3.one * objectData.scale;
            
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
    
    public void ShowDialogue(string text)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialogueText.text = text;
            dialoguePanel.SetActive(true);
        }
    }
    
    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
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
            gameController.PlayScene(nextScene);
        }
    }
    
    private Vector2 ConvertScreenPositionToCanvasPosition(Vector2 screenPercent)
    {
        if (mainCanvas == null || speakerContainer == null) return Vector2.zero;
        
        // Get the canvas rect
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return Vector2.zero;
        
        // Get canvas size
        Vector2 canvasSize = canvasRect.sizeDelta;
        
        // Convert percentage (0-1) to canvas coordinates
        // (0,0) should be bottom-left, (1,1) should be top-right
        float x = (screenPercent.x - 0.5f) * canvasSize.x;
        float y = (screenPercent.y - 0.5f) * canvasSize.y;
        
        return new Vector2(x, y);
    }
}