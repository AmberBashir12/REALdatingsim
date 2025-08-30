using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to quickly create and configure AI dialogue components
/// </summary>
public class AIDialogueSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool autoSetupScene = false;
    
    [Header("Scene Configuration")]
    [SerializeField] private string sceneName = "AI Chat Scene";
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private string aiCharacterName = "AI Assistant";
    [SerializeField] private string playerCharacterName = "Player";
    [SerializeField] private Color aiTextColor = Color.white;
    [SerializeField] private Color playerTextColor = Color.cyan;
    
    [Header("AI Configuration")]
    [TextArea(3, 5)]
    [SerializeField] private string systemPrompt = "You are a friendly character in a dating simulation game. Keep your responses natural, engaging, and around 1-2 sentences.";
    
    [TextArea(2, 4)]
    [SerializeField] private string characterDescription = "You are a charming and witty character who enjoys getting to know new people.";
    
    [SerializeField] private int maxTurns = 5;
    
    [Header("Conversation Starters")]
    [SerializeField] private string[] conversationStarters = new string[]
    {
        "Hello there! How are you doing today?",
        "I love meeting new people. What brings you here?",
        "You seem interesting. Tell me about yourself!"
    };
    
#if UNITY_EDITOR
    [ContextMenu("Create AI Scene Asset")]
    public void CreateAISceneAsset()
    {
        // Create AI Scene
        AIScene aiScene = ScriptableObject.CreateInstance<AIScene>();
        aiScene.sceneName = sceneName;
        aiScene.background = backgroundSprite;
        aiScene.systemPrompt = systemPrompt;
        aiScene.characterDescription = characterDescription;
        aiScene.maxConversationTurns = maxTurns;
        aiScene.allowPlayerInput = true;
        
        // Add conversation starters
        aiScene.conversationStarters.Clear();
        foreach (string starter in conversationStarters)
        {
            if (!string.IsNullOrEmpty(starter))
                aiScene.conversationStarters.Add(starter);
        }
        
        // Create speakers if they don't exist
        Speaker aiSpeaker = CreateSpeakerAsset(aiCharacterName, aiTextColor, "AI_Speaker");
        Speaker playerSpeaker = CreateSpeakerAsset(playerCharacterName, playerTextColor, "Player_Speaker");
        
        aiScene.aiSpeaker = aiSpeaker;
        aiScene.playerSpeaker = playerSpeaker;
        
        // Save the asset
        string path = $"Assets/1Story/Story&ChooseScenes/{sceneName}_AIScene.asset";
        AssetDatabase.CreateAsset(aiScene, path);
        
        // Also save speakers
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created AI Scene asset at: {path}");
        
        // Select the created asset
        Selection.activeObject = aiScene;
        EditorGUIUtility.PingObject(aiScene);
    }
    
    private Speaker CreateSpeakerAsset(string speakerName, Color textColor, string fileName)
    {
        string speakerPath = $"Assets/1Story/Speakers/{fileName}.asset";
        
        // Check if speaker already exists
        Speaker existingSpeaker = AssetDatabase.LoadAssetAtPath<Speaker>(speakerPath);
        if (existingSpeaker != null)
        {
            Debug.Log($"Using existing speaker: {speakerPath}");
            return existingSpeaker;
        }
        
        // Create new speaker
        Speaker speaker = ScriptableObject.CreateInstance<Speaker>();
        speaker.speakerName = speakerName;
        speaker.textColor = textColor;
        
        AssetDatabase.CreateAsset(speaker, speakerPath);
        Debug.Log($"Created speaker asset at: {speakerPath}");
        
        return speaker;
    }
    
    [ContextMenu("Setup Current Scene for AI Dialogue")]
    public void SetupCurrentSceneForAI()
    {
        // Find or create necessary GameObjects
        GameObject gameController = GameObject.FindObjectOfType<GameController>()?.gameObject;
        if (gameController == null)
        {
            gameController = new GameObject("GameController");
            gameController.AddComponent<GameController>();
            gameController.AddComponent<GeminiAPIHandler>();
        }
        
        GameObject aiDialogueController = GameObject.FindObjectOfType<AIDialogueController>()?.gameObject;
        if (aiDialogueController == null)
        {
            aiDialogueController = new GameObject("AIDialogueController");
            aiDialogueController.AddComponent<AIDialogueController>();
        }
        
        // Setup background controller
        GameObject backgroundController = GameObject.Find("BackgroundController");
        if (backgroundController == null)
        {
            backgroundController = new GameObject("BackgroundController");
            backgroundController.AddComponent<SpriteSwitcher>();
        }
        
        Debug.Log("AI Dialogue scene setup complete! Don't forget to:");
        Debug.Log("1. Set up your Canvas with UI elements");
        Debug.Log("2. Assign UI references in AIDialogueController");
        Debug.Log("3. Set your Gemini API key in GeminiAPIHandler");
        Debug.Log("4. Create and assign an AI Scene asset to GameController");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AIDialogueSetupHelper))]
public class AIDialogueSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        AIDialogueSetupHelper helper = (AIDialogueSetupHelper)target;
        
        if (GUILayout.Button("Create AI Scene Asset", GUILayout.Height(30)))
        {
            helper.CreateAISceneAsset();
        }
        
        if (GUILayout.Button("Setup Current Scene for AI Dialogue", GUILayout.Height(30)))
        {
            helper.SetupCurrentSceneForAI();
        }
        
        GUILayout.Space(5);
        EditorGUILayout.HelpBox("1. Configure the settings above\n2. Click 'Create AI Scene Asset' to generate the scene\n3. Click 'Setup Current Scene' to add necessary GameObjects\n4. Set up your UI Canvas manually\n5. Assign references and API key", MessageType.Info);
    }
}
#endif
