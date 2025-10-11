using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractiveObjectController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Interactive Settings")]
    public GameScene nextScene;
    public AudioClip soundOnClick;
    public string requiredChoiceKey;
    public string choiceKeyToUnlock;
    
    [Header("Visual Effects")]
    public Color glowColor = Color.white;
    public float glowIntensity = 1.5f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ExplorationController explorationController;
    private bool isInteractable = true;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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
            
            if (!isInteractable && spriteRenderer != null)
            {
                // Make object appear disabled
                Color disabledColor = originalColor;
                disabledColor.a = 0.5f;
                spriteRenderer.color = disabledColor;
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            Debug.Log($"Object requires choice key '{requiredChoiceKey}' to interact");
            return;
        }
        
        Debug.Log($"Interactive object clicked: {gameObject.name}");
        
        // Play sound effect
        if (soundOnClick != null && explorationController != null)
        {
            explorationController.PlaySound(soundOnClick);
        }
        
        // Unlock choice key if specified
        if (!string.IsNullOrEmpty(choiceKeyToUnlock) && GameStateManager.Instance != null)
        {
            Debug.Log($"Unlocking choice key: '{choiceKeyToUnlock}'");
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
        if (!isInteractable) return;
        
        if (spriteRenderer != null)
        {
            // Apply glow effect
            Color glowEffect = originalColor * glowIntensity;
            glowEffect.a = originalColor.a;
            spriteRenderer.color = glowEffect;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (spriteRenderer != null)
        {
            // Remove glow effect
            spriteRenderer.color = originalColor;
        }
    }
}
