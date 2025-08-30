using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAIScene", menuName = "Data/New AI Scene")]
[System.Serializable]
public class AIScene : GameScene
{
    [Header("AI Scene Settings")]
    public string sceneName;
    public Sprite background;
    public GameScene nextScene;
    
    [Header("AI Character Configuration")]
    public Speaker aiSpeaker; // The AI character
    public Speaker playerSpeaker; // The player character
    
    [Header("AI Prompt Configuration")]
    [TextArea(3, 10)]
    public string systemPrompt = "You are a character in a dating simulation game. Respond naturally and stay in character. Keep responses conversational and engaging, around 1-2 sentences.";
    
    [TextArea(2, 5)]
    public string characterDescription = "You are a friendly and charming character who enjoys getting to know new people.";
    
    [Header("Conversation Settings")]
    public int maxConversationTurns = 10;
    public bool allowPlayerInput = true;
    
    [Header("Predefined Options (Optional)")]
    public List<string> conversationStarters = new List<string>();
    public List<string> responseOptions = new List<string>();
    
    /// <summary>
    /// Get the full prompt to send to the AI including system context and character description
    /// </summary>
    public string GetFullPrompt(string playerMessage, List<string> conversationHistory = null)
    {
        string prompt = systemPrompt + "\n\n";
        prompt += "Character Description: " + characterDescription + "\n\n";
        
        if (conversationHistory != null && conversationHistory.Count > 0)
        {
            prompt += "Previous conversation:\n";
            foreach (string message in conversationHistory)
            {
                prompt += message + "\n";
            }
            prompt += "\n";
        }
        
        prompt += "Player says: \"" + playerMessage + "\"\n\n";
        prompt += "Respond as this character (keep it natural and conversational):";
        
        return prompt;
    }
}
