using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameScene currentScene;
    public BottomBarController bottomBar;
    public BackgroundSwitcher backgroundController;
    public ChooseController chooseController;
    public AudioController audioController;
    public ExplorationController explorationController;
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    private Image fadeImage;
    private int pendingStoryStartSentenceIndex = -1;

    private State state = State.IDLE;
    private State prePauseState = State.IDLE;
    public SaveSceneCatalog saveCatalog;
    public bool IsTransitioning => state == State.ANIMATE || (state == State.PAUSED && prePauseState == State.ANIMATE);

    public enum State
    {
        IDLE, ANIMATE, CHOOSE, EXPLORE, PAUSED
    }

    public State GetCurrentState()
    {
        return state;
    }

    public bool IsPaused()
    {
        return state == State.PAUSED;
    }

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            if (state != State.PAUSED)
            {
                prePauseState = state;
            }
            state = State.PAUSED;
        }
        else
        {
            state = prePauseState;
        }
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return null;
        SceneSlotData saved = SceneSaveSlots.TakePendingLoad();
        Sprite loadedBackground = null;
        if (saved != null)
        {
            SaveSceneCatalog.Entry entry = saveCatalog != null ? saveCatalog.Find(saved.sceneId) : null;
            if (entry == null)
            {
                Debug.LogError("The saved scene is missing from the save catalog. Loading stopped.");
                yield break;
            }
            currentScene = entry.scene;
            PlayerProfile.SetName(saved.playerName);
            PlayerProfile.SetCharacterChoice(saved.characterChoice);
            if (GameStateManager.Instance == null)
            {
                new GameObject("GameStateManager").AddComponent<GameStateManager>();
            }
            GameStateManager.Instance.ResetGameState();
            foreach (string key in saved.choices ?? new string[0])
            {
                GameStateManager.Instance.UnlockChoice(key);
            }
            SaveSceneCatalog.Entry preview = saveCatalog.Find(saved.backgroundSceneId);
            loadedBackground = entry.Background != null ? entry.Background : preview?.Background;
            if (preview != null && preview.Background != null)
            {
                backgroundController.SetImage(preview.Background);
            }
        }
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadeImage = fadePanel.GetComponent<Image>();
        }
        StartCoroutine(FadeInFromBlack());
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            bottomBar.PlayScene(storyScene);
            backgroundController.SetImage(storyScene.background);
            state = bottomBar.IsWaitingForChoice ? State.CHOOSE : State.IDLE;
        }
        else if (currentScene is ChooseScene chooseScene)
        {
            bottomBar.Hide();
            state = State.CHOOSE;
            chooseController.SetupChoose(chooseScene);
        }
        else if (currentScene is ExplorationScene explorationScene)
        {
            bottomBar.Hide();
            state = State.EXPLORE;
            explorationController.SetupExplorationScene(explorationScene);
        }
        if (saved != null && loadedBackground != null)
        {
            backgroundController.Image2.sprite = loadedBackground;
        }
    }

    private IEnumerator FadeInFromBlack()
    {
        yield return FadePanel(1f, 0f, disableOnComplete: true);
    }

    private IEnumerator FadeOutToBlack()
    {
        yield return FadePanel(0f, 1f, disableOnComplete: false);
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha, bool disableOnComplete)
    {
        if (fadePanel == null)
        {
            yield break;
        }

        fadePanel.gameObject.SetActive(true);
        SetFadeAlpha(startAlpha);

        float elapsedTime = 0f;
        if (fadeDuration <= 0f)
        {
            SetFadeAlpha(endAlpha);
        }
        else
        {
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
                SetFadeAlpha(Mathf.Lerp(startAlpha, endAlpha, progress));
                yield return null;
            }
        }

        SetFadeAlpha(endAlpha);

        if (disableOnComplete)
        {
            fadePanel.gameObject.SetActive(false);
        }
    }

    private void SetFadeAlpha(float alpha)
    {
        fadePanel.alpha = alpha;

        // Fallback for cases where CanvasGroup alpha isn't affecting the panel Image.
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
    // Story dialogue input is handled by BottomBarController.
    // Keeping input handling in one place avoids frame-order dependent double-advances.

    public void PlayScene(GameScene scene)
    {
        PlayScene(scene, -1, false);
    }

    public void SetChooseState(bool isChoosing)
    {
        if (isChoosing)
        {
            state = State.CHOOSE;
        }
        else if (state == State.CHOOSE)
        {
            state = State.IDLE;
        }
    }

    public void PlayScene(GameScene scene, int startSentenceIndex)
    {
        PlayScene(scene, startSentenceIndex, false);
    }

    public void PlayScene(GameScene scene, int startSentenceIndex, bool useStoryFadeTransition)
    {
        pendingStoryStartSentenceIndex = startSentenceIndex;
        StartCoroutine(SwitchScene(scene, useStoryFadeTransition));
    }

    private IEnumerator SwitchScene(GameScene scene, bool useStoryFadeTransition)
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

        if (useStoryFadeTransition)
        {
            yield return StartCoroutine(FadeOutToBlack());
        }

        currentScene = scene;
        // Hide bottom bar only for non-story scenes
        if (!(scene is StoryScene))
        {
            bottomBar.Hide();
        }
        yield return new WaitForSeconds(1f);

        if (scene is StoryScene storyScene)
        {
            if (storyScene.background == null) {
                Debug.LogWarning($"StoryScene '{storyScene.name}' has no background assigned.");
            }
            backgroundController.SwitchImage(storyScene.background);

            yield return new WaitForSeconds(0.5f);
            bottomBar.Show();
            bottomBar.ClearText();
            // Reset exploration dialogue panel when returning to story
            if (explorationController != null)
            {
                explorationController.ResetDialoguePanel();
            }
            yield return new WaitForSeconds(0.5f);
            
            bottomBar.PlayScene(storyScene, pendingStoryStartSentenceIndex); // PlayScene in BottomBarController will handle empty sentences
            pendingStoryStartSentenceIndex = -1;
            state = bottomBar.IsWaitingForChoice ? State.CHOOSE : State.IDLE;
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

        if (useStoryFadeTransition)
        {
            yield return StartCoroutine(FadeInFromBlack());
        }
    }

    public void PlayAudio(StoryScene.Sentence sentence)
    {
        audioController.PlayAudio(sentence.music, sentence.music2, sentence.sound);
    }
}
