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
    void Start()
    {
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
