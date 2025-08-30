using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AIDialogueController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public TMP_InputField playerInputField;
    public Button sendButton;
    public Button[] responseOptionButtons; // For predefined response options
    public GameObject inputPanel;
    public GameObject responsePanel;
    public Button continueButton; // To move to next scene
    
    [Header("AI Components")]
    public GeminiAPIHandler geminiAPI;
    
    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;
    public Color playerTextColor = Color.blue;
    public Color aiTextColor = Color.white;
    
    private AIScene currentScene;
    private List<string> conversationHistory = new List<string>();
    private int currentTurn = 0;
    private bool isWaitingForAI = false;
    private bool isTyping = false;
    
    private enum DialogueState
    {
        WaitingForPlayer,
        WaitingForAI,
        ShowingAIResponse,
        ConversationComplete
    }
    
    private DialogueState currentState = DialogueState.WaitingForPlayer;
    
    private void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendPlayerMessage);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToNextScene);
            
        // Setup response option buttons
        for (int i = 0; i < responseOptionButtons.Length; i++)
        {
            int buttonIndex = i; // Capture for closure
            responseOptionButtons[i].onClick.AddListener(() => SelectResponseOption(buttonIndex));
        }
        
        if (playerInputField != null)
        {
            playerInputField.onSubmit.AddListener((string text) => {
                if (!string.IsNullOrEmpty(text) && currentState == DialogueState.WaitingForPlayer)
                    SendPlayerMessage();
            });
        }
    }
    
    public void StartAIScene(AIScene scene)
    {
        currentScene = scene;
        conversationHistory.Clear();
        currentTurn = 0;
        currentState = DialogueState.WaitingForPlayer;
        
        // Setup UI
        if (speakerNameText != null)
            speakerNameText.text = "";
            
        dialogueText.text = "";
        
        // Show conversation starter or setup for player input
        if (scene.conversationStarters.Count > 0)
        {
            ShowConversationStarters();
        }
        else
        {
            SetupPlayerInput();
        }
        
        // Hide continue button initially
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }
    
    private void ShowConversationStarters()
    {
        inputPanel.SetActive(false);
        responsePanel.SetActive(true);
        
        // Setup response buttons with conversation starters
        for (int i = 0; i < responseOptionButtons.Length && i < currentScene.conversationStarters.Count; i++)
        {
            responseOptionButtons[i].gameObject.SetActive(true);
            responseOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentScene.conversationStarters[i];
        }
        
        // Hide unused buttons
        for (int i = currentScene.conversationStarters.Count; i < responseOptionButtons.Length; i++)
        {
            responseOptionButtons[i].gameObject.SetActive(false);
        }
        
        dialogueText.text = "Choose how to start the conversation:";
        speakerNameText.text = "Narrator";
    }
    
    private void SetupPlayerInput()
    {
        if (currentScene.allowPlayerInput)
        {
            inputPanel.SetActive(true);
            responsePanel.SetActive(false);
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
        else if (currentScene.responseOptions.Count > 0)
        {
            ShowResponseOptions();
        }
    }
    
    private void ShowResponseOptions()
    {
        inputPanel.SetActive(false);
        responsePanel.SetActive(true);
        
        for (int i = 0; i < responseOptionButtons.Length && i < currentScene.responseOptions.Count; i++)
        {
            responseOptionButtons[i].gameObject.SetActive(true);
            responseOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentScene.responseOptions[i];
        }
        
        for (int i = currentScene.responseOptions.Count; i < responseOptionButtons.Length; i++)
        {
            responseOptionButtons[i].gameObject.SetActive(false);
        }
    }
    
    private void SelectResponseOption(int optionIndex)
    {
        string selectedResponse;
        
        if (currentTurn == 0 && currentScene.conversationStarters.Count > optionIndex)
        {
            selectedResponse = currentScene.conversationStarters[optionIndex];
        }
        else if (currentScene.responseOptions.Count > optionIndex)
        {
            selectedResponse = currentScene.responseOptions[optionIndex];
        }
        else
        {
            return;
        }
        
        ProcessPlayerMessage(selectedResponse);
    }
    
    private void SendPlayerMessage()
    {
        if (isWaitingForAI || isTyping || string.IsNullOrEmpty(playerInputField.text))
            return;
            
        string playerMessage = playerInputField.text.Trim();
        playerInputField.text = "";
        
        ProcessPlayerMessage(playerMessage);
    }
    
    private void ProcessPlayerMessage(string playerMessage)
    {
        // Add player message to history
        string playerEntry = $"Player: {playerMessage}";
        conversationHistory.Add(playerEntry);
        
        // Display player message
        StartCoroutine(DisplayMessage(playerMessage, currentScene.playerSpeaker, playerTextColor));
        
        // Hide input UI
        inputPanel.SetActive(false);
        responsePanel.SetActive(false);
        
        // Get AI response
        currentState = DialogueState.WaitingForAI;
        RequestAIResponse(playerMessage);
    }
    
    private void RequestAIResponse(string playerMessage)
    {
        isWaitingForAI = true;
        
        string fullPrompt = currentScene.GetFullPrompt(playerMessage, conversationHistory);
        
        geminiAPI.GenerateResponse(
            fullPrompt,
            OnAIResponseReceived,
            OnAIError
        );
    }
    
    private void OnAIResponseReceived(string aiResponse)
    {
        isWaitingForAI = false;
        
        // Clean up the AI response
        aiResponse = aiResponse.Trim();
        
        // Add AI response to history
        string aiEntry = $"AI: {aiResponse}";
        conversationHistory.Add(aiEntry);
        
        // Display AI response
        StartCoroutine(DisplayMessage(aiResponse, currentScene.aiSpeaker, aiTextColor, () => {
            currentTurn++;
            CheckConversationEnd();
        }));
        
        currentState = DialogueState.ShowingAIResponse;
    }
    
    private void OnAIError(string error)
    {
        isWaitingForAI = false;
        Debug.LogError($"AI Error: {error}");
        
        // Show error message
        StartCoroutine(DisplayMessage($"[Error getting AI response: {error}]", null, Color.red, () => {
            CheckConversationEnd();
        }));
    }
    
    private IEnumerator DisplayMessage(string message, Speaker speaker, Color textColor, System.Action onComplete = null)
    {
        isTyping = true;
        
        // Set speaker name and color
        if (speaker != null)
        {
            speakerNameText.text = speaker.speakerName;
            speakerNameText.color = speaker.textColor;
        }
        else
        {
            speakerNameText.text = "";
        }
        
        // Type out the message
        dialogueText.text = "";
        dialogueText.color = textColor;
        
        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.text += message[i];
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        onComplete?.Invoke();
    }
    
    private void CheckConversationEnd()
    {
        if (currentTurn >= currentScene.maxConversationTurns)
        {
            currentState = DialogueState.ConversationComplete;
            ShowContinueButton();
        }
        else
        {
            currentState = DialogueState.WaitingForPlayer;
            // Wait a moment then show input options again
            StartCoroutine(WaitAndShowInput());
        }
    }
    
    private IEnumerator WaitAndShowInput()
    {
        yield return new WaitForSeconds(1f);
        SetupPlayerInput();
    }
    
    private void ShowContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
        inputPanel.SetActive(false);
        responsePanel.SetActive(false);
    }
    
    private void ContinueToNextScene()
    {
        if (currentScene.nextScene != null)
        {
            // Find the GameController and switch to next scene
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.PlayScene(currentScene.nextScene);
            }
        }
        else
        {
            Debug.LogWarning("No next scene assigned to AI Scene");
        }
    }
    
    // Public method to check if conversation is complete (for GameController)
    public bool IsConversationComplete()
    {
        return currentState == DialogueState.ConversationComplete;
    }
    
    // Public method to check if currently processing
    public bool IsProcessing()
    {
        return isWaitingForAI || isTyping;
    }
}
