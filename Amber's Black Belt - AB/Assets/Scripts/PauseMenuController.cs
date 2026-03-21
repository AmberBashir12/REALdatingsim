using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private CanvasGroup pausePanel;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private bool isPaused;

    private void Awake()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SetPauseUI(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (gameController != null)
        {
            gameController.SetPaused(true);
        }

        SetPauseUI(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (gameController != null)
        {
            gameController.SetPaused(false);
        }

        SetPauseUI(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void SetPauseUI(bool visible)
    {
        if (pausePanel == null)
        {
            return;
        }

        pausePanel.gameObject.SetActive(visible);
        pausePanel.alpha = visible ? 1f : 0f;
        pausePanel.interactable = visible;
        pausePanel.blocksRaycasts = visible;
    }
}
