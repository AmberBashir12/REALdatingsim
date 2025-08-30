using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quick setup script for AI dialogue scenes
/// Add this to your GameController and click the setup buttons
/// </summary>
public class QuickAISceneSetup : MonoBehaviour
{
    [Header("Quick Setup for AI Scenes")]
    [Space(10)]
    [Tooltip("Click this button to automatically create and assign required components")]
    public bool setupAIScene = false;
    
    void Start()
    {
        if (setupAIScene)
        {
            SetupAISceneComponents();
        }
    }
    
    [ContextMenu("Setup AI Scene Components")]
    public void SetupAISceneComponents()
    {
        GameController gameController = GetComponent<GameController>();
        if (gameController == null)
        {
            Debug.LogError("This script must be on the same GameObject as GameController!");
            return;
        }
        
        Debug.Log("Setting up AI Scene components...");
        
        // 1. Setup AIDialogueController if missing
        if (gameController.aiDialogueController == null)
        {
            AIDialogueController existingController = FindObjectOfType<AIDialogueController>();
            if (existingController == null)
            {
                GameObject aiControllerGO = new GameObject("AIDialogueController");
                existingController = aiControllerGO.AddComponent<AIDialogueController>();
                Debug.Log("✅ Created AIDialogueController");
            }
            gameController.aiDialogueController = existingController;
            Debug.Log("✅ Assigned AIDialogueController to GameController");
        }
        
        // 2. Setup BackgroundController if missing
        if (gameController.backgroundController == null)
        {
            SpriteSwitcher existingSwitcher = FindObjectOfType<SpriteSwitcher>();
            if (existingSwitcher == null)
            {
                GameObject bgControllerGO = new GameObject("BackgroundController");
                existingSwitcher = bgControllerGO.AddComponent<SpriteSwitcher>();
                
                // Create background images if Canvas exists
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    CreateBackgroundImages(existingSwitcher, canvas);
                }
                else
                {
                    Debug.LogWarning("⚠️ No Canvas found. You'll need to create background images manually.");
                }
                
                Debug.Log("✅ Created BackgroundController with SpriteSwitcher");
            }
            gameController.backgroundController = existingSwitcher;
            Debug.Log("✅ Assigned BackgroundController to GameController");
        }
        
        // 3. Add GeminiAPIHandler if missing
        GeminiAPIHandler apiHandler = GetComponent<GeminiAPIHandler>();
        if (apiHandler == null)
        {
            apiHandler = gameObject.AddComponent<GeminiAPIHandler>();
            Debug.Log("✅ Added GeminiAPIHandler to GameController");
        }
        
        // 4. Connect GeminiAPIHandler to AIDialogueController
        if (gameController.aiDialogueController != null)
        {
            // We'll need to set this manually since the field might be private
            Debug.Log("ℹ️ Don't forget to assign GeminiAPIHandler to AIDialogueController in the Inspector!");
        }
        
        Debug.Log("🎉 AI Scene setup complete! Next steps:");
        Debug.Log("1. Create an AI Scene asset (Right-click → Create → Data → New AI Scene)");
        Debug.Log("2. Assign the AI Scene to GameController's Current Scene field");
        Debug.Log("3. Set up UI elements (Canvas, TextMeshPro, Buttons, InputField)");
        Debug.Log("4. Assign UI references in AIDialogueController");
        Debug.Log("5. Set your Gemini API key in GeminiAPIHandler");
    }
    
    private void CreateBackgroundImages(SpriteSwitcher spriteSwitcher, Canvas canvas)
    {
        // Create background images if they don't exist
        if (spriteSwitcher.Image1 == null)
        {
            GameObject img1GO = new GameObject("BackgroundImage1");
            img1GO.transform.SetParent(canvas.transform, false);
            Image img1 = img1GO.AddComponent<Image>();
            
            // Set it to stretch to fill the screen
            RectTransform rt1 = img1GO.GetComponent<RectTransform>();
            rt1.anchorMin = Vector2.zero;
            rt1.anchorMax = Vector2.one;
            rt1.offsetMin = Vector2.zero;
            rt1.offsetMax = Vector2.zero;
            
            spriteSwitcher.Image1 = img1;
            Debug.Log("✅ Created BackgroundImage1");
        }
        
        if (spriteSwitcher.Image2 == null)
        {
            GameObject img2GO = new GameObject("BackgroundImage2");
            img2GO.transform.SetParent(canvas.transform, false);
            Image img2 = img2GO.AddComponent<Image>();
            
            // Set it to stretch to fill the screen
            RectTransform rt2 = img2GO.GetComponent<RectTransform>();
            rt2.anchorMin = Vector2.zero;
            rt2.anchorMax = Vector2.one;
            rt2.offsetMin = Vector2.zero;
            rt2.offsetMax = Vector2.zero;
            
            // Start with this one invisible
            img2.color = new Color(1, 1, 1, 0);
            
            spriteSwitcher.Image2 = img2;
            Debug.Log("✅ Created BackgroundImage2");
        }
    }
    
    [ContextMenu("Create Basic UI for AI Scene")]
    public void CreateBasicUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Create a Canvas first.");
            return;
        }
        
        Debug.Log("Creating basic UI for AI dialogue...");
        
        // Create main dialogue panel
        GameObject dialoguePanel = CreateUIPanel(canvas, "DialoguePanel", new Vector2(0, 0), new Vector2(1, 0.3f));
        
        // Create dialogue text
        CreateTextMeshPro(dialoguePanel, "DialogueText", "Click to start conversation...", 18);
        
        // Create speaker name text
        CreateTextMeshPro(dialoguePanel, "SpeakerName", "Speaker", 14);
        
        // Create input panel
        GameObject inputPanel = CreateUIPanel(canvas, "InputPanel", new Vector2(0, 0.3f), new Vector2(1, 0.4f));
        
        // Create input field (you'll need to add TMP_InputField manually)
        Debug.Log("ℹ️ Add a TMP_InputField to the InputPanel manually");
        
        // Create buttons panel
        GameObject buttonPanel = CreateUIPanel(canvas, "ButtonPanel", new Vector2(0, 0.7f), new Vector2(1, 1f));
        
        Debug.Log("✅ Basic UI structure created! You still need to:");
        Debug.Log("1. Add TMP_InputField to InputPanel");
        Debug.Log("2. Add Buttons for send/continue/options");
        Debug.Log("3. Assign all UI references in AIDialogueController");
    }
    
    private GameObject CreateUIPanel(Canvas canvas, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Add a background image (optional)
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        
        return panel;
    }
    
    private GameObject CreateTextMeshPro(GameObject parent, string name, string text, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);
        
        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10, 10);
        rt.offsetMax = new Vector2(-10, -10);
        
        // Note: You'll need TextMeshPro imported to use this
        // For now, we'll use regular Text
        UnityEngine.UI.Text textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        
        Debug.Log($"ℹ️ Created {name} - consider replacing with TextMeshPro for better quality");
        
        return textGO;
    }
}
