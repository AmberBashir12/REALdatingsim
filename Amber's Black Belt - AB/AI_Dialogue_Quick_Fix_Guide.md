# 🛠️ Quick Fix Guide for AI Dialogue System Errors

## ❌ Common Errors and Solutions

### 1. **NullReferenceException in GameController.Start() line 25**
**Problem:** `backgroundController` is null

**Solutions:**
1. **Check GameController Assignment:**
   - Select your GameController GameObject in the scene
   - In the Inspector, make sure "Background Controller" field is assigned
   - Drag the GameObject with SpriteSwitcher component to this field

2. **Create Background Controller if Missing:**
   ```
   - Create empty GameObject named "BackgroundController"
   - Add SpriteSwitcher component to it
   - Assign it to GameController's Background Controller field
   ```

### 2. **NullReferenceException in SpriteSwitcher.SetImage() line 38**
**Problem:** `Image1` or `Image2` in SpriteSwitcher is null

**Solutions:**
1. **Setup UI Images:**
   ```
   - Create Canvas if not exists
   - Create two UI Image objects under Canvas
   - Name them "BackgroundImage1" and "BackgroundImage2"
   - Assign them to SpriteSwitcher's Image1 and Image2 fields
   ```

2. **Quick Setup Steps:**
   - Right-click in Hierarchy → UI → Canvas
   - Right-click Canvas → UI → Image (do this twice)
   - Select BackgroundController GameObject
   - Drag the two Image objects to Image1 and Image2 fields

### 3. **Missing AI Dialogue Controller**
**Problem:** AIDialogueController not assigned to GameController

**Solution:**
```
- Create empty GameObject named "AIDialogueController"
- Add AIDialogueController script to it
- Assign it to GameController's "AI Dialogue Controller" field
```

## 🔧 Complete Scene Setup Checklist

### Step 1: Create GameObjects
```
Hierarchy should have:
├── Canvas
│   ├── DialoguePanel
│   │   ├── Background (Image)
│   │   ├── SpeakerName (TextMeshPro - UI)
│   │   └── DialogueText (TextMeshPro - UI)
│   ├── InputPanel
│   │   ├── PlayerInputField (TMP_InputField)
│   │   └── SendButton (Button)
│   ├── ResponsePanel
│   │   ├── OptionButton1 (Button)
│   │   ├── OptionButton2 (Button)
│   │   ├── OptionButton3 (Button)
│   │   └── OptionButton4 (Button)
│   ├── ContinueButton (Button)
│   ├── BackgroundImage1 (Image)
│   └── BackgroundImage2 (Image)
├── GameController (Empty)
├── BackgroundController (Empty)
└── AIDialogueController (Empty)
```

### Step 2: Add Components
- **GameController:** GameController, GeminiAPIHandler scripts
- **BackgroundController:** SpriteSwitcher, Animator components
- **AIDialogueController:** AIDialogueController script

### Step 3: Assign References
**In GameController:**
- Current Scene → Your AI Scene asset
- Background Controller → BackgroundController GameObject
- AI Dialogue Controller → AIDialogueController GameObject

**In SpriteSwitcher (on BackgroundController):**
- Image1 → BackgroundImage1
- Image2 → BackgroundImage2

**In AIDialogueController:**
- Dialogue Text → DialogueText TextMeshPro
- Speaker Name Text → SpeakerName TextMeshPro
- Player Input Field → PlayerInputField
- Send Button → SendButton
- Response Option Buttons → Array of OptionButtons
- Input Panel → InputPanel GameObject
- Response Panel → ResponsePanel GameObject
- Continue Button → ContinueButton
- Gemini API → GeminiAPIHandler component on GameController

### Step 4: Create Scene Asset
- Right-click in Project → Create → Data → New AI Scene
- Configure speakers, prompts, and conversation settings
- Assign to GameController's Current Scene field

## 🚀 Quick Testing Steps

1. **Add Setup Checker:**
   - Add AISystemSetupChecker script to GameController
   - Click "Run Setup Check" button in Inspector
   - Follow the console messages to fix any issues

2. **Test API Connection:**
   - Add GeminiAPITester script to any GameObject
   - Set your Gemini API key in GeminiAPIHandler
   - Click "Test API" to verify connection

## 🔍 Debugging Tips

1. **Enable Console Logging:**
   - Window → General → Console
   - Clear on Play enabled
   - Collapse disabled to see all messages

2. **Check Component Assignments:**
   - Select each GameObject
   - Look for "None (ComponentName)" in Inspector
   - These indicate missing assignments

3. **Common Missing References:**
   - GameController.backgroundController
   - GameController.aiDialogueController
   - SpriteSwitcher.Image1/Image2
   - AIDialogueController UI references

4. **Verify Scene Type:**
   - Make sure Current Scene is an AI Scene asset
   - Check that AI Scene has speakers assigned

## 💡 Pro Tips

- **Use Prefabs:** Save configured UI as prefab for reuse
- **Test Incrementally:** Set up basic components first, then add features
- **Console is Your Friend:** Read error messages carefully
- **Inspector Validation:** Red/pink fields indicate missing references

If you're still having issues after following this guide, the AISystemSetupChecker script will give you detailed information about what's missing!
