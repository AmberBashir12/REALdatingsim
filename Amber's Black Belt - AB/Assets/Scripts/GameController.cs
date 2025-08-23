using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NarrativeRuntime;
using Narrative;

public class GameController : MonoBehaviour
{
    [Header("Narrative Data")] public string narrativeResourcePath = "Narrative/exported_narrative"; // Resources path without extension
    public string overrideStartSceneId;

    [Header("Controllers")] public BottomBarController bottomBar;
    public SpriteSwitcher backgroundController;
    public ChooseController chooseController;

    private IGameScene currentScene;
    private State state = State.IDLE;

    private enum State { IDLE, ANIMATE, CHOOSE }

    private void Start()
    {
        LoadNarrative();
        var startId = string.IsNullOrEmpty(overrideStartSceneId) ? NarrativeRegistry.StartSceneId : overrideStartSceneId;
        if (string.IsNullOrEmpty(startId))
        {
            Debug.LogError("Runtime narrative start scene id is null/empty. Check YAML 'startScene' or loader errors above.");
            return;
        }
        if (!string.IsNullOrEmpty(startId) && NarrativeRegistry.TryGet(startId, out var startScene))
        {
            PlayScene(startScene);
        }
        else
        {
            Debug.LogError($"Start scene id '{startId}' not found or narrative not loaded. Falling back to legacy currentScene if assigned.");
            if (currentSceneLegacy != null)
            {
                PlayLegacyScene(currentSceneLegacy);
            }
        }
    }

    [Header("Legacy (temporary)")] public StoryScene currentSceneLegacy;

    private void Update()
    {
        // Hot reload (Ctrl+R)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Hot reloading narrative...");
            LoadNarrative();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (state == State.IDLE && bottomBar.IsCompleted())
            {
                if (bottomBar.IsLastSentence())
                {
                    if (currentScene is RuntimeStoryScene rss)
                    {
                        if (!string.IsNullOrEmpty(rss.NextId) && NarrativeRegistry.TryGet(rss.NextId, out var next))
                        {
                            PlayScene(next);
                        }
                        else
                        {
                            Debug.Log("End of branch or missing next id.");
                        }
                    }
                }
                else
                {
                    bottomBar.PlayNextSentence();
                }
            }
        }
    }

    private void LoadNarrative()
    {
    NarrativeLoader.Load(narrativeResourcePath);
    }

    public void PlayScene(IGameScene scene)
    {
        StartCoroutine(SwitchScene(scene));
    }

    // Legacy support for ScriptableObject StoryScene while migrating
    public void PlayLegacyScene(StoryScene scene)
    {
        StartCoroutine(LegacyStoryFlow(scene));
    }

    private IEnumerator LegacyStoryFlow(StoryScene storyScene)
    {
        if (storyScene == null) yield break;
        state = State.ANIMATE;
        bottomBar.Hide();
        yield return new WaitForSeconds(0.3f);
        if (storyScene.background)
        {
            backgroundController.SwitchImage(storyScene.background);
        }
        yield return new WaitForSeconds(0.3f);
        bottomBar.Show();
        bottomBar.ClearText();
        yield return new WaitForSeconds(0.1f);
        bottomBar.PlayScene(storyScene);
        state = State.IDLE;
    }

    private IEnumerator SwitchScene(IGameScene scene)
    {
        state = State.ANIMATE;
        if (scene == null)
        {
            Debug.LogError("Null scene passed to SwitchScene");
            state = State.IDLE;
            yield break;
        }
        currentScene = scene;
        bottomBar.Hide();
        yield return new WaitForSeconds(0.5f);
        if (scene.Background != null)
        {
            backgroundController.SwitchImage(scene.Background);
        }
        yield return new WaitForSeconds(0.5f);
        bottomBar.Show();
        bottomBar.ClearText();
        yield return new WaitForSeconds(0.2f);

        if (scene is RuntimeStoryScene rss)
        {
            bottomBar.PlayRuntimeStory(rss);
            state = State.IDLE;
        }
        else if (scene is RuntimeChooseScene rcs)
        {
            state = State.CHOOSE;
            chooseController.SetupRuntimeChoose(rcs);
        }
    }
}
