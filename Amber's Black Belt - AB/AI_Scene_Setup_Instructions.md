# AI Dialogue Test Scene Setup Instructions

## Prerequisites
1. Open Unity and load the project
2. Ensure you have a Gemini API key from Google AI Studio (https://makersuite.google.com/app/apikey)

## Setting up the AI Test Scene

### 1. Create a new Scene
1. In Unity, go to File > New Scene
2. Save the scene as "AITestScene" in the Assets/Scenes folder

### 2. Setup the Scene Hierarchy
Create the following GameObjects in your scene:

```
AITestScene
├── Main Camera
├── EventSystem
├── Canvas
│   ├── DialoguePanel
│   │   ├── Background (Image)
│   │   ├── SpeakerName (TextMeshPro)
│   │   └── DialogueText (TextMeshPro)
│   ├── InputPanel
│   │   ├── PlayerInputField (TMP_InputField)
│   │   └── SendButton (Button)
│   ├── ResponsePanel
│   │   ├── OptionButton1 (Button)
│   │   ├── OptionButton2 (Button)
│   │   ├── OptionButton3 (Button)
│   │   └── OptionButton4 (Button)
│   └── ContinueButton (Button)
├── GameController (Empty GameObject)
├── BackgroundController (Empty GameObject)
└── AIDialogueController (Empty GameObject)
```

### 3. Setup Components

#### GameController GameObject:
- Add `GameController` script
- Add `GeminiAPIHandler` script
- Add `SpriteSwitcher` script (for background)

#### AIDialogueController GameObject:
- Add `AIDialogueController` script

### 4. Configure the UI
- Set Canvas to Screen Space - Overlay
- Position DialoguePanel at the bottom of the screen
- Style the text components with appropriate fonts and colors
- Set up the input field and buttons

### 5. Wire up the Components
In the AIDialogueController:
- Assign all UI references (dialogueText, speakerNameText, etc.)
- Set typing speed to 0.05
- Set player and AI text colors

In the GameController:
- Assign the AIDialogueController reference
- Assign the BackgroundController (SpriteSwitcher component)

### 6. Create AI Scene Asset
1. Right-click in Project window
2. Create > Data > New AI Scene
3. Name it "TestAIScene"
4. Configure the scene with:
   - Scene name: "AI Chat Test"
   - AI Speaker: Create or assign a Speaker asset
   - Player Speaker: Create or assign a Speaker asset
   - System Prompt: "You are a friendly AI character in a dating simulation. Keep responses natural and engaging."
   - Character Description: "You are an enthusiastic AI who loves to chat and get to know people."
   - Max conversation turns: 5
   - Add conversation starters like "Hello there!", "How are you doing?", "What's your favorite hobby?"

### 7. Set API Key
1. Select the GameController in the scene
2. In the GeminiAPIHandler component, paste your Gemini API key
3. IMPORTANT: Never commit your API key to version control!

### 8. Test the Scene
1. Set the GameController's Current Scene to your TestAIScene asset
2. Play the scene
3. Try the conversation starters and free-form input

## Usage Notes
- The system will maintain conversation history for context
- Responses are generated dynamically by the Gemini AI
- The conversation will end after the specified number of turns
- Use the Continue button to move to the next scene (if assigned)

## Troubleshooting
- If API calls fail, check your API key and internet connection
- Ensure all UI references are properly assigned
- Check the Console for any error messages
- Make sure TextMeshPro is imported (Window > TextMeshPro > Import TMP Essential Resources)

## Example Conversation Flow
1. Scene starts with conversation starter options
2. Player selects a starter or types custom message
3. AI generates contextual response
4. Player can continue conversation
5. After max turns, continue button appears to move to next scene
