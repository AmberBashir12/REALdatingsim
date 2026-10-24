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

    private static bool IsPaused = false;

    void Awake()
    {
        // The flag is static, so it survives scene loads such as returning to the main menu while paused.
        IsPaused = false;

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
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
    }
}
