using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection.Emit;
using System.Drawing;
using UnityEditor.SearchService;

public class ChooseController : MonoBehaviour
{

    public ChooseLabelController label;
    public GameObject labelPrefab;
    public GameController gameController;
    private RectTransform rectTransform;
    public Animator animator;
    private float labelHeight = 70f;  // Fixed height for consistency
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;  // Start invisible
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
      
    }

    public void SetupChoose(ChooseScene scene)
    {
        DestroyLabels();
      
        animator.SetTrigger("Show");
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Get all available choices (base + unlocked additional choices)
        List<ChooseScene.ChooseLabel> availableChoices = scene.GetAvailableChoices();

        for (int i = 0; i < availableChoices.Count; i++) 
        {
            ChooseLabelController newLabel = Instantiate(labelPrefab, transform).GetComponent<ChooseLabelController>();
            // Enable the ChooseLabelController script
            newLabel.enabled = true;
            
            // Enable the TextMeshPro component
            TextMeshProUGUI tmpText = newLabel.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.enabled = true;
            }
            
            float yPos = CalculateLabelPosition(availableChoices.Count, i);
            newLabel.Setup(availableChoices[i], this, yPos);
            newLabel.SetChooseScene(scene); // Pass the ChooseScene so it can handle choice key unlocking
        }

        // Adjust container height based on number of choices
        float totalHeight = (availableChoices.Count + 1) * labelHeight;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalHeight);
    }

    public void PerformChoose(StoryScene scene)
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameController.PlayScene(scene);
        animator.SetTrigger("Hide");
    }

    private float CalculateLabelPosition(int labelCount, int labelIndex)
    {
        float spacing = labelHeight * 1.2f; // Add 20% spacing between choices
        float totalHeight = spacing * (labelCount - 1);
        float startY = totalHeight - 300;
        
        return startY - (labelIndex * spacing);
    }

    private void DestroyLabels()
    {
        foreach (Transform childTransform in transform)
        {
           Destroy(childTransform.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
