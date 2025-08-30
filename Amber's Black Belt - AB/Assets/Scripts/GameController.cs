using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameScene currentScene;
    public BottomBarController bottomBar;
    public SpriteSwitcher backgroundController;
    public ChooseController chooseController;
    public AIDialogueController aiDialogueController; // Add AI dialogue controller reference

    private State state = State.IDLE;

    private enum State
    {
        IDLE, ANIMATE, CHOOSE, AI_DIALOGUE
    }
    // Start is called before the first frame update
    void Start()
    {
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            
            // BottomBar is required for StoryScenes
            if (bottomBar != null)
            {
                bottomBar.PlayScene(storyScene);
            }
            else
            {
                Debug.LogError("BottomBarController not assigned to GameController! This is required for StoryScenes.");
            }
            
            // Background controller is optional for StoryScenes
            if (backgroundController != null && storyScene.background != null)
            {
                backgroundController.SetImage(storyScene.background);
            }
            else if (backgroundController == null)
            {
                Debug.LogWarning("Background Controller not assigned to GameController! Background images won't work.");
            }
        }
        else if (currentScene is AIScene)
        {
            AIScene aiScene = currentScene as AIScene;
            
            // Background controller is optional for AI scenes
            if (backgroundController != null && aiScene.background != null)
            {
                backgroundController.SetImage(aiScene.background);
            }
            else if (backgroundController == null && aiScene.background != null)
            {
                Debug.LogWarning("Background Controller not assigned but AI scene has a background. Background won't display.");
            }
            
            // AI Dialogue Controller is required for AI scenes
            if (aiDialogueController != null)
            {
                aiDialogueController.StartAIScene(aiScene);
                state = State.AI_DIALOGUE;
            }
            else
            {
                Debug.LogError("AIDialogueController not assigned to GameController! This is required for AI scenes.");
            }
        }
        else if (currentScene == null)
        {
            Debug.LogError("No scene assigned to GameController! Please assign a scene in the Inspector.");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (state == State.IDLE && bottomBar.IsCompleted())
            {
                if (bottomBar.IsLastSentence())
                {
                    PlayScene((currentScene as StoryScene).nextScene);
                }
                else
                {
                    bottomBar.PlayNextSentence();
                }
            }
        }
        
        // Check if AI dialogue is complete
        if (state == State.AI_DIALOGUE && aiDialogueController != null)
        {
            if (aiDialogueController.IsConversationComplete() && !aiDialogueController.IsProcessing())
            {
                // AI conversation is done, but don't auto-advance - let the continue button handle it
            }
        }
    }

    public void PlayScene(GameScene scene)
    {
        StartCoroutine(SwitchScene(scene));
    }

    private IEnumerator SwitchScene(GameScene scene)
    {
        state = State.ANIMATE;

        if (scene == null)
        {
            Debug.LogError($"Attempted to switch to a null scene. Current scene was '{currentScene?.name}'. Check 'nextScene' assignments in your StoryScene assets.");
            if (bottomBar.IsHidden) // Ensure bottom bar is visible if we can't proceed
            {
                bottomBar.Show();
            }
            state = State.IDLE; // Revert to IDLE to allow player interaction or prevent soft lock
            yield break; // Exit coroutine
        }

        currentScene = scene;  
        bottomBar.Hide();
        yield return new WaitForSeconds(1f);

        if (scene is StoryScene storyScene)
        {
            if (storyScene.background == null) {
                Debug.LogWarning($"StoryScene '{storyScene.name}' has no background assigned.");
            }
            backgroundController.SwitchImage(storyScene.background);
            
            yield return new WaitForSeconds(1f);
            bottomBar.Show();
            bottomBar.ClearText();
            yield return new WaitForSeconds(1f);
            
            bottomBar.PlayScene(storyScene); // PlayScene in BottomBarController will handle empty sentences
            state = State.IDLE; // Reset state to IDLE
        }
        else if (scene is ChooseScene chooseScene) 
        {
            state = State.CHOOSE;
            chooseController.SetupChoose(chooseScene); 
        }
        else if (scene is AIScene aiScene)
        {
            if (aiScene.background != null)
            {
                backgroundController.SwitchImage(aiScene.background);
            }
            
            yield return new WaitForSeconds(1f);
            
            if (aiDialogueController != null)
            {
                aiDialogueController.StartAIScene(aiScene);
                state = State.AI_DIALOGUE;
            }
            else
            {
                Debug.LogError("AIDialogueController not found! Cannot start AI scene.");
                state = State.IDLE;
            }
        }
        else
        {
            Debug.LogError($"Loaded scene '{scene.name}' is not a StoryScene, ChooseScene, or AIScene. Type: {scene.GetType()}. Cannot proceed.");
            if (bottomBar.IsHidden)
            {
                bottomBar.Show();
            }
            state = State.IDLE; // Revert to IDLE
        }
    }
}
