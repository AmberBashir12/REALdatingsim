using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractiveObjectController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Interactive Settings")]
    public GameScene nextScene;
    public AudioClip soundOnClick;
    public string requiredChoiceKey;
    public string choiceKeyToUnlock;
    
    [Header("Visual Effects")]
    public Color glowColor = Color.red;
    public float glowIntensity = 3f;
    
    private Image imageComponent;
    private Outline outlineComponent;
    private Color originalColor;
    private ExplorationController explorationController;
    private bool isInteractable = true;
    
    void Start()
    {
        // Get Image component (for UI)
        imageComponent = GetComponent<Image>();
        
        // Get Outline component for glow effect
        outlineComponent = GetComponent<Outline>();
        
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
        
        // Find the exploration controller
        explorationController = FindObjectOfType<ExplorationController>();
        
        // Check if this object should be interactable based on choice keys
        CheckInteractability();
    }
    
    public void Setup(GameScene scene, AudioClip sound, string reqKey, string unlockKey)
    {
        nextScene = scene;
        soundOnClick = sound;
        requiredChoiceKey = reqKey;
        choiceKeyToUnlock = unlockKey;
        CheckInteractability();
    }
    
    private void CheckInteractability()
    {
        if (!string.IsNullOrEmpty(requiredChoiceKey) && GameStateManager.Instance != null)
        {
            isInteractable = GameStateManager.Instance.IsChoiceUnlocked(requiredChoiceKey);
            
            if (!isInteractable && imageComponent != null)
            {
                // Make object appear disabled
                Color disabledColor = originalColor;
                disabledColor.a = 0.5f;
                imageComponent.color = disabledColor;
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }
        
        // Play sound effect
        if (soundOnClick != null && explorationController != null)
        {
            explorationController.PlaySound(soundOnClick);
        }
        
        // Unlock choice key if specified
        if (!string.IsNullOrEmpty(choiceKeyToUnlock) && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockChoice(choiceKeyToUnlock);
        }
        
        // Navigate to next scene
        if (nextScene != null && explorationController != null)
        {
            explorationController.NavigateToScene(nextScene);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Enable outline glow effect
        if (outlineComponent != null)
        {
            outlineComponent.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Disable outline glow effect
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }
}
