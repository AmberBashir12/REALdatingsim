using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameScene currentScene;
    public BottomBarController bottomBar;
    public BackgroundSwitcher backgroundController;
    public ChooseController chooseController;
    public AudioController audioController;
    public ExplorationController explorationController;

    private State state = State.IDLE;

    private enum State
    {
        IDLE, ANIMATE, CHOOSE, EXPLORE
    }
    // Start is called before the first frame update
    void Start()
    {
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            bottomBar.PlayScene(storyScene);
            backgroundController.SetImage(storyScene.background);
            PlayAudio(storyScene.sentences[0]);
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
                    PlayScene((currentScene as StoryScene).GetNextScene());
                }
                else
                {
                    bottomBar.PlayNextSentence();
                    PlayAudio((currentScene as StoryScene).sentences[bottomBar.GetSentenceIndex()]);
                }
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
            PlayAudio(storyScene.sentences[0]);

            yield return new WaitForSeconds(1f);
            bottomBar.Show();
            bottomBar.ClearText();
            // Reset exploration dialogue panel when returning to story
            if (explorationController != null)
            {
                explorationController.ResetDialoguePanel();
            }
            yield return new WaitForSeconds(1f);
            
            bottomBar.PlayScene(storyScene); // PlayScene in BottomBarController will handle empty sentences
            state = State.IDLE; // Reset state to IDLE
        }
        else if (scene is ChooseScene chooseScene) 
        {
            state = State.CHOOSE;
            chooseController.SetupChoose(chooseScene); 
        }
        else if (scene is ExplorationScene explorationScene)
        {
            state = State.EXPLORE;
            explorationController.SetupExplorationScene(explorationScene);
        }
        else
        {
            Debug.LogError($"Loaded scene '{scene.name}' is not a StoryScene, ChooseScene, or ExplorationScene. Type: {scene.GetType()}. Cannot proceed.");
            if (bottomBar.IsHidden)
            {
                bottomBar.Show();
            }
            state = State.IDLE; // Revert to IDLE
        }
    }

    private void PlayAudio(StoryScene.Sentence sentence)
    {
        audioController.PlayAudio(sentence.music, sentence.sound);
    } 
}
