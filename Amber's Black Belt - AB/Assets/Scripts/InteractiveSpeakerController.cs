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
    
    private UnityEngine.UI.Image imageComponent;
    private Color originalColor;
    private ExplorationController explorationController;
    private Speaker speaker;
    
    void Start()
    {
        imageComponent = GetComponent<UnityEngine.UI.Image>();
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
        else
        {
            Debug.LogWarning($"InteractiveSpeakerController on {gameObject.name} has no Image component!");
        }
    }
    
    public void Setup(string dialogue, ExplorationController controller, Speaker speakerData)
    {
        dialogueText = dialogue;
        explorationController = controller;
        speaker = speakerData;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Speaker clicked: {gameObject.name}");
        
        if (!string.IsNullOrEmpty(dialogueText) && explorationController != null)
        {
            explorationController.ShowDialogue(dialogueText, speaker);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (imageComponent != null)
        {
            // Apply glow effect
            Color glowEffect = originalColor * glowIntensity;
            glowEffect.a = originalColor.a;
            imageComponent.color = glowEffect;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (imageComponent != null)
        {
            // Remove glow effect
            imageComponent.color = originalColor;
        }
    }
}