using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple runtime setup checker for AI dialogue system
/// Add this to any GameObject and it will automatically check setup on Start
/// </summary>
public class SimpleAISetupChecker : MonoBehaviour
{
    [Header("Setup Checker")]
    [Tooltip("Check this to run the setup check automatically when the scene starts")]
    public bool checkOnStart = true;
    
    [Tooltip("Check this to display results in UI instead of just console")]
    public bool showUIResults = false;
    
    [Header("UI Display (Optional)")]
    public TextMeshProUGUI resultDisplay;
    
    private string results = "";
    
    void Start()
    {
        if (checkOnStart)
        {
            CheckSetup();
        }
    }
    
    public void CheckSetup()
    {
        results = "";
        AddResult("=== AI DIALOGUE SYSTEM SETUP CHECK ===\n");
        
        CheckGameController();
        CheckSpriteSwitcher();
        CheckAIDialogueController();
        CheckUIComponents();
        CheckAPIHandler();
        
        AddResult("\n=== SETUP CHECK COMPLETE ===");
        
        // Display results
        Debug.Log(results);
        
        if (showUIResults && resultDisplay != null)
        {
            resultDisplay.text = results;
        }
    }
    
    private void AddResult(string message)
    {
        results += message + "\n";
    }
    
    private void CheckGameController()
    {
        AddResult("--- GameController Check ---");
        
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController == null)
        {
            AddResult("❌ GameController not found in scene!");
            return;
        }
        
        AddResult("✅ GameController found");
        
        // Check current scene
        if (gameController.currentScene == null)
            AddResult("❌ Current Scene not assigned!");
        else
            AddResult("✅ Current Scene: " + gameController.currentScene.name);
            
        // Check background controller
        if (gameController.backgroundController == null)
            AddResult("❌ Background Controller not assigned!");
        else
            AddResult("✅ Background Controller assigned");
            
        // Check AI dialogue controller
        if (gameController.aiDialogueController == null)
            AddResult("❌ AI Dialogue Controller not assigned!");
        else
            AddResult("✅ AI Dialogue Controller assigned");
    }
    
    private void CheckSpriteSwitcher()
    {
        AddResult("\n--- SpriteSwitcher Check ---");
        
        SpriteSwitcher spriteSwitcher = FindObjectOfType<SpriteSwitcher>();
        if (spriteSwitcher == null)
        {
            AddResult("❌ SpriteSwitcher not found!");
            return;
        }
        
        AddResult("✅ SpriteSwitcher found");
        
        if (spriteSwitcher.Image1 == null)
            AddResult("❌ Image1 not assigned!");
        else
            AddResult("✅ Image1 assigned");
            
        if (spriteSwitcher.Image2 == null)
            AddResult("❌ Image2 not assigned!");
        else
            AddResult("✅ Image2 assigned");
            
        Animator animator = spriteSwitcher.GetComponent<Animator>();
        if (animator == null)
            AddResult("⚠️ Animator missing (may cause issues)");
        else
            AddResult("✅ Animator found");
    }
    
    private void CheckAIDialogueController()
    {
        AddResult("\n--- AIDialogueController Check ---");
        
        AIDialogueController aiController = FindObjectOfType<AIDialogueController>();
        if (aiController == null)
        {
            AddResult("❌ AIDialogueController not found!");
            AddResult("Create GameObject and add AIDialogueController script");
            return;
        }
        
        AddResult("✅ AIDialogueController found");
        AddResult("ℹ️ Check Inspector for UI reference assignments:");
        AddResult("  - Dialogue Text, Speaker Name Text");
        AddResult("  - Input Field, Send Button");
        AddResult("  - Response Buttons, Panels");
        AddResult("  - Gemini API reference");
    }
    
    private void CheckUIComponents()
    {
        AddResult("\n--- UI Components Check ---");
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            AddResult("❌ No Canvas found!");
        else
            AddResult("✅ Canvas found");
            
        TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>();
        AddResult($"ℹ️ TextMeshPro components: {textComponents.Length}");
        
        TMP_InputField inputField = FindObjectOfType<TMP_InputField>();
        if (inputField == null)
            AddResult("⚠️ No TMP_InputField found");
        else
            AddResult("✅ TMP_InputField found");
            
        Button[] buttons = FindObjectsOfType<Button>();
        AddResult($"ℹ️ Button components: {buttons.Length}");
        
        if (buttons.Length < 2)
            AddResult("⚠️ Need at least 2 buttons (Send + Continue)");
    }
    
    private void CheckAPIHandler()
    {
        AddResult("\n--- API Handler Check ---");
        
        GeminiAPIHandler apiHandler = FindObjectOfType<GeminiAPIHandler>();
        if (apiHandler == null)
        {
            AddResult("❌ GeminiAPIHandler not found!");
            AddResult("Add GeminiAPIHandler script to GameController");
        }
        else
        {
            AddResult("✅ GeminiAPIHandler found");
            AddResult("ℹ️ Remember to set your API key!");
        }
    }
    
    // Public method to manually trigger check (can be called from button)
    public void RunManualCheck()
    {
        CheckSetup();
    }
}
