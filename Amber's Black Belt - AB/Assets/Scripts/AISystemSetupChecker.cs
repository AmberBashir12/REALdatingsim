using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Debug helper to check if all components are properly assigned for AI dialogue system
/// Attach this to your GameController and run it to identify missing assignments
/// </summary>
public class AISystemSetupChecker : MonoBehaviour
{
    [Header("Click the button below to check your setup")]
    [SerializeField] private bool runCheck = false;
    
    void Start()
    {
        if (runCheck)
        {
            CheckSetup();
        }
    }
    
    [ContextMenu("Check AI System Setup")]
    public void CheckSetup()
    {
        Debug.Log("=== AI DIALOGUE SYSTEM SETUP CHECK ===");
        
        GameController gameController = GetComponent<GameController>();
        if (gameController == null)
        {
            Debug.LogError("❌ GameController component not found!");
            return;
        }
        
        CheckGameController(gameController);
        CheckSpriteSwitcher();
        CheckAIDialogueController();
        CheckUIComponents();
        CheckSceneAsset(gameController);
        
        Debug.Log("=== SETUP CHECK COMPLETE ===");
    }
    
    private void CheckGameController(GameController gameController)
    {
        Debug.Log("--- GameController Check ---");
        
        if (gameController.currentScene == null)
            Debug.LogError("❌ Current Scene not assigned in GameController!");
        else
            Debug.Log("✅ Current Scene assigned: " + gameController.currentScene.name);
            
        if (gameController.backgroundController == null)
            Debug.LogError("❌ Background Controller not assigned in GameController!");
        else
            Debug.Log("✅ Background Controller assigned");
            
        if (gameController.aiDialogueController == null)
            Debug.LogError("❌ AI Dialogue Controller not assigned in GameController!");
        else
            Debug.Log("✅ AI Dialogue Controller assigned");
            
        if (gameController.bottomBar == null)
            Debug.LogWarning("⚠️ Bottom Bar Controller not assigned (needed for StoryScenes)");
        else
            Debug.Log("✅ Bottom Bar Controller assigned");
            
        if (gameController.chooseController == null)
            Debug.LogWarning("⚠️ Choose Controller not assigned (needed for ChooseScenes)");
        else
            Debug.Log("✅ Choose Controller assigned");
    }
    
    private void CheckSpriteSwitcher()
    {
        Debug.Log("--- SpriteSwitcher Check ---");
        
        SpriteSwitcher spriteSwitcher = FindObjectOfType<SpriteSwitcher>();
        if (spriteSwitcher == null)
        {
            Debug.LogError("❌ SpriteSwitcher component not found in scene!");
            return;
        }
        
        if (spriteSwitcher.Image1 == null)
            Debug.LogError("❌ Image1 not assigned in SpriteSwitcher!");
        else
            Debug.Log("✅ Image1 assigned in SpriteSwitcher");
            
        if (spriteSwitcher.Image2 == null)
            Debug.LogError("❌ Image2 not assigned in SpriteSwitcher!");
        else
            Debug.Log("✅ Image2 assigned in SpriteSwitcher");
            
        Animator animator = spriteSwitcher.GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("❌ Animator component missing on SpriteSwitcher!");
        else
            Debug.Log("✅ Animator component found on SpriteSwitcher");
    }
    
    private void CheckAIDialogueController()
    {
        Debug.Log("--- AIDialogueController Check ---");
        
        AIDialogueController aiController = FindObjectOfType<AIDialogueController>();
        if (aiController == null)
        {
            Debug.LogError("❌ AIDialogueController not found in scene!");
            return;
        }
        
        // Use reflection to check private fields or make them public for testing
        Debug.Log("ℹ️ AIDialogueController found. Check the Inspector to ensure all UI references are assigned:");
        Debug.Log("   - dialogueText (TextMeshProUGUI)");
        Debug.Log("   - speakerNameText (TextMeshProUGUI)");
        Debug.Log("   - playerInputField (TMP_InputField)");
        Debug.Log("   - sendButton (Button)");
        Debug.Log("   - responseOptionButtons (Button[])");
        Debug.Log("   - inputPanel (GameObject)");
        Debug.Log("   - responsePanel (GameObject)");
        Debug.Log("   - continueButton (Button)");
        Debug.Log("   - geminiAPI (GeminiAPIHandler)");
    }
    
    private void CheckUIComponents()
    {
        Debug.Log("--- UI Components Check ---");
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            Debug.LogError("❌ No Canvas found in scene!");
        else
            Debug.Log("✅ Canvas found");
            
        TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>();
        Debug.Log($"ℹ️ Found {textComponents.Length} TextMeshPro components");
        
        TMP_InputField inputField = FindObjectOfType<TMP_InputField>();
        if (inputField == null)
            Debug.LogWarning("⚠️ No TMP_InputField found for player input");
        else
            Debug.Log("✅ TMP_InputField found for player input");
            
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log($"ℹ️ Found {buttons.Length} Button components");
        
        if (buttons.Length < 3)
            Debug.LogWarning("⚠️ You might need more buttons (Send, Continue, Response Options)");
    }
    
    private void CheckSceneAsset(GameController gameController)
    {
        Debug.Log("--- Scene Asset Check ---");
        
        if (gameController.currentScene == null)
        {
            Debug.LogError("❌ No scene asset assigned!");
            return;
        }
        
        if (gameController.currentScene is AIScene aiScene)
        {
            Debug.Log("✅ AI Scene asset detected");
            
            if (aiScene.aiSpeaker == null)
                Debug.LogError("❌ AI Speaker not assigned in AI Scene!");
            else
                Debug.Log("✅ AI Speaker assigned: " + aiScene.aiSpeaker.speakerName);
                
            if (aiScene.playerSpeaker == null)
                Debug.LogError("❌ Player Speaker not assigned in AI Scene!");
            else
                Debug.Log("✅ Player Speaker assigned: " + aiScene.playerSpeaker.speakerName);
                
            if (string.IsNullOrEmpty(aiScene.systemPrompt))
                Debug.LogWarning("⚠️ System prompt is empty in AI Scene!");
            else
                Debug.Log("✅ System prompt configured");
                
            if (string.IsNullOrEmpty(aiScene.characterDescription))
                Debug.LogWarning("⚠️ Character description is empty in AI Scene!");
            else
                Debug.Log("✅ Character description configured");
                
            Debug.Log($"ℹ️ Max conversation turns: {aiScene.maxConversationTurns}");
            Debug.Log($"ℹ️ Conversation starters: {aiScene.conversationStarters.Count}");
        }
        else if (gameController.currentScene is StoryScene)
        {
            Debug.Log("ℹ️ Story Scene detected (not AI scene)");
        }
        else
        {
            Debug.Log("ℹ️ Other scene type detected: " + gameController.currentScene.GetType().Name);
        }
    }
    
    private void CheckGeminiAPI()
    {
        Debug.Log("--- Gemini API Check ---");
        
        GeminiAPIHandler apiHandler = FindObjectOfType<GeminiAPIHandler>();
        if (apiHandler == null)
        {
            Debug.LogError("❌ GeminiAPIHandler not found in scene!");
            return;
        }
        
        Debug.Log("✅ GeminiAPIHandler found");
        Debug.Log("ℹ️ Make sure to set your API key in the GeminiAPIHandler component!");
        Debug.Log("ℹ️ You can test the API connection using the GeminiAPITester script");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AISystemSetupChecker))]
public class AISystemSetupCheckerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        AISystemSetupChecker checker = (AISystemSetupChecker)target;
        
        if (GUILayout.Button("🔍 Run Setup Check", GUILayout.Height(40)))
        {
            checker.CheckSetup();
        }
        
        GUILayout.Space(5);
        EditorGUILayout.HelpBox("This will check if all components are properly assigned for the AI dialogue system. Check the Console for detailed results.", MessageType.Info);
    }
}
#endif
