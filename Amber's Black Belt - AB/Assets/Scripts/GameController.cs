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
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    private Image fadeImage;
    private int pendingStoryStartSentenceIndex = -1;

    private State state = State.IDLE;
    private State prePauseState = State.IDLE;
    public SaveSceneCatalog saveCatalog;

    public string[] SceneStartChoices { get; private set; } = new string[0];
    public bool IsTransitioning => state == State.ANIMATE || (state == State.PAUSED && prePauseState == State.ANIMATE);

    public enum State
    {
        IDLE, ANIMATE, CHOOSE, PAUSED
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
        StopMusicIfRequested(currentScene);
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadeImage = fadePanel.GetComponent<Image>();
        }
        CaptureSceneStartChoices();
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
        if (saved != null && loadedBackground != null)
        {
            backgroundController.Image2.sprite = loadedBackground;
        }
    }

    private void CaptureSceneStartChoices()
    {
        SceneStartChoices = GameStateManager.Instance != null
            ? GameStateManager.Instance.GetUnlockedChoices() : new string[0];
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


        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }



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
            if (bottomBar.IsHidden)
            {
                bottomBar.Show();
            }
            state = State.IDLE;
            yield break;
        }

        if (useStoryFadeTransition)
        {
            yield return StartCoroutine(FadeOutToBlack());
        }

        currentScene = scene;
        StopMusicIfRequested(scene);
        CaptureSceneStartChoices();

        if (!(scene is StoryScene))
        {
            bottomBar.Hide();
        }
        yield return new WaitForSeconds(0.3f);

        if (scene is StoryScene storyScene)
        {
            backgroundController.SwitchImage(storyScene.background);

            yield return new WaitForSeconds(0.25f);
            bottomBar.Show();
            bottomBar.ClearText();
            yield return new WaitForSeconds(0.25f);
            
            bottomBar.PlayScene(storyScene, pendingStoryStartSentenceIndex);
            pendingStoryStartSentenceIndex = -1;
            state = bottomBar.IsWaitingForChoice ? State.CHOOSE : State.IDLE;
        }
        else if (scene is ChooseScene chooseScene) 
        {
            state = State.CHOOSE;
            chooseController.SetupChoose(chooseScene); 
        }
        else
        {
            if (bottomBar.IsHidden)
            {
                bottomBar.Show();
            }
            state = State.IDLE;
        }

        if (useStoryFadeTransition)
        {
            yield return StartCoroutine(FadeInFromBlack());
        }
    }

    private void StopMusicIfRequested(GameScene scene)
    {
        if (scene is StoryScene storyScene && storyScene.stopMusicOnStart)
        {
            if (audioController == null)
            {
                return;
            }

            audioController.StopMusic();
        }
    }

    public void PlayAudio(StoryScene.Sentence sentence)
    {
        audioController.PlayAudio(sentence.music, sentence.music2, sentence.sound);
    }
}
