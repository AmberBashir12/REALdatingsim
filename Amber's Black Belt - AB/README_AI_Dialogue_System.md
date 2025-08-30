# AI-Powered Dynamic Dialogue System

This system integrates Google's Gemini AI API to create dynamic, contextual dialogue in your Unity dating simulation game. Instead of pre-written dialogue, characters can respond intelligently to player input in real-time.

## Features

- **Dynamic AI Responses**: Characters respond contextually using Google's Gemini 2.0 Flash model
- **Conversation History**: AI maintains context throughout the conversation
- **Flexible Input**: Support for both free-form text input and predefined response options
- **Conversation Management**: Configurable turn limits and conversation flow
- **Unity Integration**: Seamlessly integrates with existing scene management system
- **Error Handling**: Robust error handling for API failures

## Components

### 1. GeminiAPIHandler.cs
Manages communication with Google's Gemini API:
- Handles HTTP requests to Gemini API
- Manages API key security
- Provides callbacks for success/error handling
- Supports custom prompts and context

### 2. AIScene.cs
ScriptableObject that defines an AI-powered dialogue scene:
- Character configuration (AI speaker, player speaker)
- System prompts and character descriptions
- Conversation settings (turn limits, input modes)
- Predefined conversation starters and response options

### 3. AIDialogueController.cs
Main controller for AI dialogue gameplay:
- Manages conversation flow and state
- Handles UI interactions (text input, buttons)
- Coordinates between player input and AI responses
- Maintains conversation history for context

### 4. Updated GameController.cs
Extended to support AI scenes:
- Recognizes AIScene types
- Manages transitions between different scene types
- Coordinates background and UI changes

## Setup Instructions

### 1. Get Gemini API Key
1. Visit [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Create an account and generate an API key
3. Copy your API key (keep it secure!)

### 2. Create AI Scene Assets
```csharp
// Right-click in Project > Create > Data > New AI Scene
// Configure with:
- Scene name
- Background sprite
- AI and Player speakers
- System prompt and character description
- Conversation settings
```

### 3. Setup Scene Hierarchy
```
AITestScene
├── Canvas
│   ├── DialoguePanel (with TextMeshPro components)
│   ├── InputPanel (with TMP_InputField and Button)
│   ├── ResponsePanel (with multiple choice buttons)
│   └── ContinueButton
├── GameController (with GeminiAPIHandler)
└── AIDialogueController
```

### 4. Configure Components
- Assign UI references in AIDialogueController
- Set your API key in GeminiAPIHandler
- Wire up the GameController references

## Usage Examples

### Basic AI Scene Configuration
```csharp
// In your AIScene asset:
systemPrompt = "You are a charming character in a dating sim. Keep responses natural and engaging.";
characterDescription = "You're a coffee shop owner who loves books and rainy days.";
maxConversationTurns = 8;
conversationStarters = new List<string> 
{
    "Hello! I love your coffee shop!",
    "What's your favorite book?",
    "This weather is perfect for reading, isn't it?"
};
```

### Custom API Prompts
The system automatically builds prompts with context:
```
[System Prompt]

Character Description: [Your character description]

Previous conversation:
Player: Hello! I love your coffee shop!
AI: Thank you! I put a lot of love into this place. Do you have a favorite type of coffee?

Player says: "I love espresso!"

Respond as this character (keep it natural and conversational):
```

## API Integration Details

### Request Format
```json
{
  "contents": [
    {
      "parts": [
        {
          "text": "Your constructed prompt with context"
        }
      ]
    }
  ]
}
```

### Headers Required
```
Content-Type: application/json
X-goog-api-key: YOUR_API_KEY
```

### Response Handling
The system extracts text from the API response and handles errors gracefully:
```csharp
// Success: Display AI response and continue conversation
// Error: Show error message and allow continuation
```

## Best Practices

### 1. Prompt Engineering
- Keep system prompts clear and specific
- Include character personality in descriptions
- Set expectations for response length
- Provide context about the game world

### 2. Conversation Management
- Limit conversation turns to maintain engagement
- Provide conversation starters to guide players
- Use conversation history for better context

### 3. Error Handling
- Always provide fallback responses for API failures
- Test with invalid API keys during development
- Include user-friendly error messages

### 4. Performance
- Cache responses when possible
- Implement loading indicators for API calls
- Consider rate limiting for API usage

## Security Considerations

⚠️ **Important Security Notes:**
- Never commit API keys to version control
- Use environment variables or Unity's ScriptableObject for API key storage
- Consider server-side API calls for production games
- Monitor API usage and costs

## Troubleshooting

### Common Issues
1. **API Key Errors**: Verify key is correct and has proper permissions
2. **Network Errors**: Check internet connection and firewall settings
3. **JSON Parsing Errors**: Ensure response format is as expected
4. **UI Not Responding**: Verify all UI references are assigned

### Debug Tips
- Enable Unity Console logging
- Test API connection with GeminiAPITester script
- Use breakpoints in OnAIResponseReceived/OnAIError methods
- Check API rate limits and quotas

## Integration with Existing Systems

The AI dialogue system is designed to work alongside your existing dialogue system:
- Traditional StoryScene assets continue to work unchanged
- ChooseScene assets remain functional
- GameController automatically detects scene types
- Smooth transitions between AI and traditional scenes

## Example Conversation Flow

1. **Scene Start**: Player sees conversation starter options
2. **Player Input**: Selects starter or types custom message
3. **AI Processing**: System sends contextual prompt to Gemini API
4. **AI Response**: Generated response is displayed with typing effect
5. **Conversation Continues**: Player can respond, AI maintains context
6. **Scene End**: After max turns, continue button appears for next scene

This system opens up endless possibilities for dynamic, engaging dialogue that adapts to each player's unique choices and conversation style!
