using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates the missing UI elements for AI dialogue system
/// Run this to quickly add the buttons and input fields you need
/// </summary>
public class CreateMissingAIUI : MonoBehaviour
{
    [Header("Click the button below to create missing UI")]
    [SerializeField] private bool createUI = false;
    
    void Start()
    {
        if (createUI)
        {
            CreateMissingUIElements();
        }
    }
    
    [ContextMenu("Create Missing AI UI Elements")]
    public void CreateMissingUIElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Please create a Canvas first.");
            return;
        }
        
        AIDialogueController aiController = FindObjectOfType<AIDialogueController>();
        if (aiController == null)
        {
            Debug.LogError("No AIDialogueController found!");
            return;
        }
        
        Debug.Log("Creating missing UI elements for AI dialogue...");
        
        // Create Response Panel (for conversation starter buttons)
        if (aiController.responsePanel == null)
        {
            GameObject responsePanel = CreatePanel(canvas, "ResponsePanel", 
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.6f));
            aiController.responsePanel = responsePanel;
            
            // Create response option buttons
            Button[] buttons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject buttonGO = CreateButton(responsePanel, $"OptionButton{i + 1}", 
                    $"Conversation Starter {i + 1}", i);
                buttons[i] = buttonGO.GetComponent<Button>();
            }
            aiController.responseOptionButtons = buttons;
            
            Debug.Log("✅ Created Response Panel with 4 option buttons");
        }
        
        // Create Input Panel (for typing custom messages)
        if (aiController.inputPanel == null)
        {
            GameObject inputPanel = CreatePanel(canvas, "InputPanel", 
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.4f));
            aiController.inputPanel = inputPanel;
            
            // Create input field
            if (aiController.playerInputField == null)
            {
                GameObject inputFieldGO = CreateInputField(inputPanel, "PlayerInputField");
                aiController.playerInputField = inputFieldGO.GetComponent<TMP_InputField>();
            }
            
            // Create send button
            if (aiController.sendButton == null)
            {
                GameObject sendButtonGO = CreateButton(inputPanel, "SendButton", "Send", -1);
                aiController.sendButton = sendButtonGO.GetComponent<Button>();
            }
            
            Debug.Log("✅ Created Input Panel with input field and send button");
        }
        
        // Create Continue Button
        if (aiController.continueButton == null)
        {
            GameObject continueButtonGO = CreateButton(canvas.gameObject, "ContinueButton", 
                "Continue to Next Scene", -1);
            aiController.continueButton = continueButtonGO.GetComponent<Button>();
            
            // Position it at the bottom
            RectTransform rt = continueButtonGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.3f, 0.05f);
            rt.anchorMax = new Vector2(0.7f, 0.15f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            // Hide it initially
            continueButtonGO.SetActive(false);
            
            Debug.Log("✅ Created Continue Button");
        }
        
        // Hide input panel initially (show response panel first)
        aiController.inputPanel.SetActive(false);
        aiController.responsePanel.SetActive(true);
        
        Debug.Log("🎉 UI setup complete! Your conversation starters should now appear!");
        Debug.Log("💡 Don't forget to set your Gemini API key in the GeminiAPIHandler component!");
    }
    
    private GameObject CreatePanel(Canvas canvas, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Add semi-transparent background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.3f);
        
        return panel;
    }
    
    private GameObject CreateButton(GameObject parent, string name, string text, int index)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent.transform, false);
        
        // Position buttons in a vertical layout
        RectTransform rt = buttonGO.AddComponent<RectTransform>();
        if (index >= 0) // Option buttons
        {
            float yPos = 0.8f - (index * 0.2f); // Stack vertically
            rt.anchorMin = new Vector2(0.1f, yPos - 0.1f);
            rt.anchorMax = new Vector2(0.9f, yPos);
        }
        else // Send button or continue button
        {
            rt.anchorMin = new Vector2(0.7f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.3f);
        }
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Add Button component
        Button button = buttonGO.AddComponent<Button>();
        
        // Add background image
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f); // Blue-ish
        
        // Add text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontSize = 14;
        
        // Try to find a font
        Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (arial != null)
            buttonText.font = arial;
        
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        return buttonGO;
    }
    
    private GameObject CreateInputField(GameObject parent, string name)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent.transform, false);
        
        RectTransform rt = inputGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.4f);
        rt.anchorMax = new Vector2(0.6f, 0.8f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Add background
        Image bg = inputGO.AddComponent<Image>();
        bg.color = Color.white;
        
        // Add TMP_InputField (this requires TextMeshPro)
        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
        
        // Create text area
        GameObject textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        RectTransform textAreaRT = textAreaGO.AddComponent<RectTransform>();
        textAreaRT.anchorMin = Vector2.zero;
        textAreaRT.anchorMax = Vector2.one;
        textAreaRT.offsetMin = new Vector2(10, 5);
        textAreaRT.offsetMax = new Vector2(-10, -5);
        
        // Create placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textAreaGO.transform, false);
        TextMeshProUGUI placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Type your message...";
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        placeholder.fontSize = 14;
        
        RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = Vector2.zero;
        placeholderRT.offsetMax = Vector2.zero;
        
        // Create input text
        GameObject inputTextGO = new GameObject("Text");
        inputTextGO.transform.SetParent(textAreaGO.transform, false);
        TextMeshProUGUI inputText = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputText.text = "";
        inputText.color = Color.black;
        inputText.fontSize = 14;
        
        RectTransform inputTextRT = inputTextGO.GetComponent<RectTransform>();
        inputTextRT.anchorMin = Vector2.zero;
        inputTextRT.anchorMax = Vector2.one;
        inputTextRT.offsetMin = Vector2.zero;
        inputTextRT.offsetMax = Vector2.zero;
        
        // Assign to input field
        inputField.textViewport = textAreaRT;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        
        return inputGO;
    }
}
