using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple test script to verify Gemini API connection
/// Attach this to a GameObject with a Button and TextMeshPro component
/// </summary>
public class GeminiAPITester : MonoBehaviour
{
    [Header("UI References")]
    public Button testButton;
    public TextMeshProUGUI resultText;
    
    [Header("API Handler")]
    public GeminiAPIHandler geminiAPI;
    
    [Header("Test Settings")]
    public string testPrompt = "Say hello and introduce yourself as an AI assistant in one sentence.";
    
    private void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestAPI);
        }
        
        if (resultText != null)
        {
            resultText.text = "Click 'Test API' to verify Gemini API connection.";
        }
    }
    
    public void TestAPI()
    {
        if (geminiAPI == null)
        {
            if (resultText != null)
                resultText.text = "Error: GeminiAPIHandler not assigned!";
            return;
        }
        
        if (resultText != null)
            resultText.text = "Testing API connection...";
        
        geminiAPI.GenerateResponse(
            testPrompt,
            OnTestSuccess,
            OnTestError
        );
    }
    
    private void OnTestSuccess(string response)
    {
        if (resultText != null)
        {
            resultText.text = $"✓ API Test Successful!\n\nAI Response:\n{response}";
            resultText.color = Color.green;
        }
        
        Debug.Log($"Gemini API Test Successful: {response}");
    }
    
    private void OnTestError(string error)
    {
        if (resultText != null)
        {
            resultText.text = $"✗ API Test Failed!\n\nError:\n{error}";
            resultText.color = Color.red;
        }
        
        Debug.LogError($"Gemini API Test Failed: {error}");
    }
}
