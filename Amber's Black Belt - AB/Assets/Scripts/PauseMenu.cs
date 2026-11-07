using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenuUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject saveSlotCanvas;

    private static bool IsPaused = false;
    private GameController gameController;

    void Awake()
    {

        IsPaused = false;
        gameController = FindObjectOfType<GameController>();
        Time.timeScale = 1f;

        if (resumeButton == null)
        {
            resumeButton = FindChildButton("ResumeButton");
        }

        if (skipButton == null)
        {
            skipButton = FindChildButton("Skip");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToNextChoice);
        }
        pauseMenuUI.SetActive(false);
        if (saveSlotCanvas != null)
        {
            saveSlotCanvas.SetActive(false);
        }
    }

    private Button FindChildButton(string childName)
    {
        Transform found = pauseMenuUI != null ? pauseMenuUI.transform.Find(childName) : null;
        return found != null ? found.GetComponent<Button>() : null;
    }

    public void SkipToNextChoice()
    {
        Resume();

        BottomBarController bottomBar = FindObjectOfType<BottomBarController>();
        if (bottomBar != null)
        {
            bottomBar.SkipToNextChoice();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (saveSlotCanvas != null)
        {
            saveSlotCanvas.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;
        if (gameController != null && gameController.IsPaused())
        {
            gameController.SetPaused(false);
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        if (gameController != null)
        {
            gameController.SetPaused(true);
        }
    }
}
