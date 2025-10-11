using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractiveSpeakerController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Speaker Settings")]
    public string dialogueText;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 1.3f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ExplorationController explorationController;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void Setup(string dialogue, ExplorationController controller)
    {
        dialogueText = dialogue;
        explorationController = controller;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Speaker clicked: {gameObject.name}");
        
        if (!string.IsNullOrEmpty(dialogueText) && explorationController != null)
        {
            explorationController.ShowDialogue(dialogueText);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
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