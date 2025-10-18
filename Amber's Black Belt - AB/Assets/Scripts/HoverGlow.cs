using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Glow Settings")]
    [Tooltip("How much brighter the object gets on hover (1.5 = 50% brighter)")]
    public float glowIntensity = 1.5f;
    
    [Tooltip("Speed of the glow transition")]
    public float transitionSpeed = 5f;
    
    [Tooltip("Use smooth transitions instead of instant")]
    public bool smoothTransition = true;
    
    private Image imageComponent;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine glowCoroutine;
    
    void Start()
    {
        // Check for UI Image first
        imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
        else
        {
            // Check for SpriteRenderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            else
            {
                Debug.LogWarning($"HoverGlow on {gameObject.name} found no Image or SpriteRenderer component!");
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (smoothTransition)
        {
            StartGlowTransition(true);
        }
        else
        {
            ApplyGlow(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (smoothTransition)
        {
            StartGlowTransition(false);
        }
        else
        {
            ApplyGlow(false);
        }
    }
    
    private void ApplyGlow(bool shouldGlow)
    {
        Color targetColor = shouldGlow ? originalColor * glowIntensity : originalColor;
        targetColor.a = originalColor.a; // Keep original transparency
        
        if (imageComponent != null)
        {
            imageComponent.color = targetColor;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
    }
    
    private void StartGlowTransition(bool shouldGlow)
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        
        Color targetColor = shouldGlow ? originalColor * glowIntensity : originalColor;
        targetColor.a = originalColor.a;
        
        glowCoroutine = StartCoroutine(SmoothColorTransition(targetColor));
    }
    
    private IEnumerator SmoothColorTransition(Color targetColor)
    {
        Color currentColor = GetCurrentColor();
        
        while (Vector4.Distance(currentColor, targetColor) > 0.01f)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
            SetCurrentColor(currentColor);
            yield return null;
        }
        
        SetCurrentColor(targetColor);
        glowCoroutine = null;
    }
    
    private Color GetCurrentColor()
    {
        if (imageComponent != null)
            return imageComponent.color;
        else if (spriteRenderer != null)
            return spriteRenderer.color;
        else
            return Color.white;
    }
    
    private void SetCurrentColor(Color color)
    {
        if (imageComponent != null)
        {
            imageComponent.color = color;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    
    // Public method to manually trigger glow (useful for other scripts)
    public void TriggerGlow(bool shouldGlow)
    {
        if (shouldGlow)
            OnPointerEnter(null);
        else
            OnPointerExit(null);
    }
    
    // Reset to original color (useful for cleanup)
    public void ResetColor()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }
        
        SetCurrentColor(originalColor);
    }
}